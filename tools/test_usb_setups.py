"""Exercise the actual embedded installer runtimes on a disposable T: drive.

Requires T: to be an existing SUBST mapping into the supplied empty test folder.
Does not start Carillon, send mail, or use the live D: installation.
"""
from pathlib import Path
from io import BytesIO
import argparse
import hashlib
import json
import shutil
import sqlite3
import struct
import subprocess
import winreg
import zipfile

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('work', type=Path)
args = parser.parse_args()
work = args.work.resolve()
assert work.is_relative_to(root/'export'), work
work.mkdir(parents=True, exist_ok=True)
mapping = subprocess.check_output(['subst'], text=True)
assert 'T:\\:' in mapping or 'T:\\' in mapping, mapping
assert str(work/'drive-root').lower() in mapping.lower(), mapping
assert not list(Path('T:/').iterdir()), 'Test drive must be empty'
key = r'Software\Microsoft\Windows\CurrentVersion\Uninstall\PackageStudioPro_Westminster Chimes and Carillon Bells 9.0'
try:
    with winreg.OpenKey(winreg.HKEY_CURRENT_USER, key):
        raise RuntimeError('A real v9 installer registration already exists; do not replace it during tests')
except FileNotFoundError:
    pass
results = []

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def run(stage, target, mode, success=True):
    project = json.loads((stage/'install.wiproj').read_text(encoding='utf-8-sig'))
    project['installFolder'] = str(target)
    (stage/'install.wiproj').write_text(json.dumps(project, indent=2), encoding='utf-8')
    result = subprocess.run([str(stage/'WipRuntime.exe'), str(stage/'install.wiproj'), '--'+mode], capture_output=True, text=True)
    (work/f'{len(results):02d}-{stage.name}-{mode}.log').write_text(result.stdout+result.stderr, encoding='utf-8')
    assert (result.returncode == 0) == success, result.stdout+result.stderr
    assert 'Target: '+str(target) in result.stdout, result.stdout
    results.append({'package': stage.name, 'target': str(target), 'mode': mode, 'expectedSuccess': success, 'exitCode': result.returncode})

def unpack(runtime, folder, kind):
    stem = f'CarillonBells_9_0_USB_{kind}_{runtime}'
    data = (root/'export'/folder/(stem+'_Package_Setup.exe')).read_bytes()
    marker = b'PSPSETUPBUNDLE1'
    assert data.endswith(marker)
    size = struct.unpack_from('<q', data, len(data)-len(marker)-8)[0]
    stage = work/stem
    with zipfile.ZipFile(BytesIO(data[-len(marker)-8-size:-len(marker)-8])) as archive:
        assert archive.testzip() is None
        assert all('..' not in Path(name).parts and ':' not in name and not name.startswith(('/', '\\')) for name in archive.namelist())
        archive.extractall(stage)
    assert (stage/'payload/CarillonBells.exe').is_file(), 'Setup must contain direct application files'
    assert not list((stage/'payload').glob('*.zip')), 'Setup still stages a ZIP'
    return stage

for runtime, folder, target in [('win-x64','64-bit',Path('T:/')), ('win-x86','32-bit',Path('T:/V9 with spaces'))]:
    new = unpack(runtime, folder, 'New_Install')
    upgrade = unpack(runtime, folder, 'Upgrade')
    assert not (upgrade/'payload/databases').exists()
    assert not (upgrade/'payload/Music').exists()
    run(new, target, 'install')
    for source in (new/'payload').rglob('*'):
        if source.is_file():
            assert digest(source) == digest(target/source.relative_to(new/'payload')), source
    assert not list(target.glob('*.zip'))
    run(new, target, 'verify')
    database = target/'databases/carillon.db'
    with sqlite3.connect(database) as connection:
        assert connection.execute('PRAGMA quick_check').fetchone()[0] == 'ok'
        connection.execute('PRAGMA user_version=1926')
    connection.close()
    personal = target/'Music/User recording.mp3'
    personal.write_bytes(b'Disposable user music sentinel')
    before = {str(p.relative_to(target)): digest(p) for directory in ['databases','Music'] for p in (target/directory).rglob('*') if p.is_file()}
    run(upgrade, target, 'install')
    after = {str(p.relative_to(target)): digest(p) for directory in ['databases','Music'] for p in (target/directory).rglob('*') if p.is_file()}
    assert before == after, 'Upgrade altered user database or music'
    run(upgrade, target, 'verify')
    guide = target/'Documentation/CarillonBells_9_0_Quick_Start_Guide.pdf'
    original = guide.read_bytes()
    guide.write_bytes(b'Deliberate test damage')
    run(upgrade, target, 'verify', success=False)
    run(upgrade, target, 'repair')
    assert guide.read_bytes() == original
    assert digest(database) == before[str(database.relative_to(target))]
    run(upgrade, target, 'verify')
    run(upgrade, target, 'uninstall')
    # Native uninstall deliberately retains changed/user files. Delete only our
    # private fixture tree after resolving it back inside the disposable export.
    resolved = (work/'drive-root'/target.relative_to(Path('T:/'))).resolve()
    assert resolved.is_relative_to(work/'drive-root')
    for child in resolved.iterdir():
        if child.is_dir(): shutil.rmtree(child)
        else: child.unlink()
    if resolved != (work/'drive-root').resolve():
        resolved.rmdir()

    legacy = Path('T:/Legacy 8.1 '+runtime)
    (legacy/'databases').mkdir(parents=True)
    (legacy/'Songs').mkdir()
    (legacy/'databases/carillon.db').write_bytes(b'Disposable existing database sentinel')
    (legacy/'Songs/user.mp3').write_bytes(b'Disposable existing audio sentinel')
    (legacy/'Carillon.exe').write_bytes(b'Disposable old executable sentinel')
    before = {str(p.relative_to(legacy)): digest(p) for p in legacy.rglob('*') if p.is_file()}
    run(upgrade, legacy, 'install')
    assert all(digest(legacy/name) == value for name, value in before.items())
    run(upgrade, legacy, 'verify')
    run(upgrade, legacy, 'uninstall')
    assert all(digest(legacy/name) == value for name, value in before.items())
    legacy_fixture = work/'drive-root'/legacy.relative_to(Path('T:/'))
    assert legacy_fixture.resolve().is_relative_to(work/'drive-root')
    shutil.rmtree(legacy_fixture)

report = {'result': 'PASS', 'scope': 'Actual embedded x86/x64 installer runtimes on disposable SUBST drive; no physical USB or Windows 10 claim', 'checks': results,
          'payloadHashes': 'All installed New Install files match embedded sources', 'upgradePreservation': 'Changed valid SQLite database, existing audio, extra user recording and legacy executable preserved',
          'failureDetection': 'Damaged guide rejected and repaired; selected folder recorded for every operation'}
(work/'package-install-tests.json').write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'PASS {len(results)} actual embedded-runtime operations plus installed payload and user-data hashes')
