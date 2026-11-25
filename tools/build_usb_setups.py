"""Build direct-file USB Setup programs from the matching, reviewed USB ZIPs.

Run after building guides and USB ZIPs. The ZIP remains an alternative download;
Setup embeds its individual files and installs them directly into the chosen folder.
"""
from pathlib import Path
import argparse
import json
import shutil
import subprocess
import zipfile

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--builder', type=Path, default=Path('C:/New Delphi Projects/Package Studio Pro/bin/WIPBuild.exe'))
args = parser.parse_args()

for runtime, folder in [('win-x86', '32-bit'), ('win-x64', '64-bit')]:
    for kind in ['New_Install', 'Upgrade']:
        stem = f'CarillonBells_9_0_USB_{kind}_{runtime}'
        release = root/'export'/folder
        source = root/'export/setup-payloads'/stem
        source.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(release/(stem+'.zip')) as archive:
            assert archive.testzip() is None
            files = [entry for entry in archive.infolist() if not entry.is_dir()]
            assert all(not Path(entry.filename).is_absolute() and '..' not in Path(entry.filename).parts and ':' not in entry.filename and '\\' not in entry.filename for entry in files)
            archive.extractall(source)
        project_path = root/'tools/package-projects'/(stem+'.wiproj')
        project = json.loads(project_path.read_text(encoding='utf-8-sig'))
        project['productName'] = 'Westminster Chimes and Carillon Bells 9.0'
        project['installFolder'] = '{localappdata}\\Westminster Chimes and Carillon Bells 9.0'
        project['files'] = [str((source/entry.filename).relative_to(root)) for entry in files]
        project_path.write_text(json.dumps(project, indent=2)+'\n', encoding='utf-8')
        output = root/'export/setup-builds'/stem
        subprocess.run([str(args.builder), str(project_path), str(output)], cwd=root, check=True)
        installers = list(output.glob('*Setup.exe'))
        assert len(installers) == 1, output
        shutil.copy2(installers[0], release/(stem+'_Package_Setup.exe'))
        print('Built direct USB Setup:', stem, flush=True)
