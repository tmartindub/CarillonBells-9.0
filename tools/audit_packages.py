"""Verify the current USB ZIPs and their matching self-contained Setup payloads."""
from pathlib import Path
from io import BytesIO
import argparse
import hashlib
import json
import struct
import zipfile

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--source-commit', required=True)
parser.add_argument('--guides', type=Path, help='Optional folder containing candidate DOCX and PDF guides')
args = parser.parse_args()
results = []

def sha(data):
    return hashlib.sha256(data).hexdigest().upper()

def machine(data):
    assert data[:2] == b'MZ'
    offset = struct.unpack_from('<I', data, 0x3c)[0]
    assert data[offset:offset+4] == b'PE\0\0'
    return struct.unpack_from('<H', data, offset+4)[0]

guides = {}
for folder, pattern in [('docs/guides', '*.docx'), ('docs/pdf', '*.pdf')]:
    for path in (args.guides if args.guides else root/folder).glob(pattern):
        guides['Documentation/'+path.name] = path.read_bytes()
assert len(guides) == 8

for runtime, folder, expected in [('win-x86', '32-bit', 0x14c), ('win-x64', '64-bit', 0x8664)]:
    release = root/'export'/folder
    for name in ['CarillonBells.exe', 'CarillonMigration.exe']:
        data = (release/name).read_bytes()
        assert machine(data) == expected, (runtime, name)
    for kind in ['New_Install', 'Upgrade']:
        stem = f'CarillonBells_9_0_USB_{kind}_{runtime}'
        path = release/(stem+'.zip')
        with zipfile.ZipFile(path) as archive:
            assert archive.testzip() is None, path.name
            names = archive.namelist()
            assert len(names) == len(set(names))
            assert all(not name.startswith(('/', '\\')) and '..' not in Path(name).parts for name in names)
            assert archive.read('CarillonBells.exe') == (release/'CarillonBells.exe').read_bytes()
            for name, data in guides.items():
                assert archive.read(name) == data, (path.name, name)
            if kind == 'Upgrade':
                assert archive.read('CarillonMigration.exe') == (release/'CarillonMigration.exe').read_bytes()
                assert set(names) == {'Documentation/', 'CarillonBells.exe', 'CarillonMigration.exe', *guides}
            else:
                allowed = {'data/', 'databases/', 'Help/', 'logs/', 'Music/', 'Documentation/', 'CarillonBells.exe', 'databases/carillon.db', *guides}
                assert all(name in allowed or name.startswith(('Help/', 'Music/')) for name in names)
                assert archive.read('databases/carillon.db')[:16] == b'SQLite format 3\0'
                assert sum(name.lower().endswith(('.mp3', '.wav')) for name in names) == 129
                for source in (root/'help').rglob('*'):
                    if source.is_file():
                        name = 'Help/'+source.relative_to(root/'help').as_posix()
                        assert archive.read(name) == source.read_bytes(), name
        setup = release/(stem+'_Package_Setup.exe')
        data = setup.read_bytes()
        marker = b'PSPSETUPBUNDLE1'
        assert data.endswith(marker), setup.name
        size = struct.unpack_from('<q', data, len(data)-len(marker)-8)[0]
        assert 0 < size < len(data)-len(marker)-8
        bundle = data[-len(marker)-8-size:-len(marker)-8]
        with zipfile.ZipFile(BytesIO(bundle)) as embedded:
            assert embedded.testzip() is None
            assert embedded.read('payload/'+path.name) == path.read_bytes(), setup.name
            project = json.loads(embedded.read('install.wiproj').decode('utf-8-sig'))
            assert not project.get('shortcuts') and not project.get('registry')
        results.append({'runtime': runtime, 'kind': kind, 'file': str(path.relative_to(root)), 'bytes': path.stat().st_size,
                        'sha256': sha(path.read_bytes()), 'setup': str(setup.relative_to(root)), 'setupSha256': sha(data),
                        'applicationSha256': sha((release/'CarillonBells.exe').read_bytes()),
                        'migrationSha256': sha((release/'CarillonMigration.exe').read_bytes()),
                        'verification': 'PASS PE architecture, ZIP CRC, current executables, eight current guides, Help and matching embedded Setup ZIP'})
manifest = {'version': '9.0.0', 'sourceCommit': args.source_commit, 'packages': results,
            'productionPromotionApproved': False, 'publicationStatus': 'Local only; do not push or publish without Tommy approval'}
(root/'source distributions/release-manifest.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
print('PASS four USB ZIPs and four matching Setup programs; release manifest refreshed.')
