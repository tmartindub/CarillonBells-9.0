"""Build direct-file USB Setup programs from the matching, reviewed USB ZIPs.

Run after building guides and portable USB ZIPs. Public downloads are ZIP files
containing Setup, a guide and START_HERE.txt. Setup installs directly onto the USB.
"""
from pathlib import Path
import argparse
import json
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
        download_folder = root/'export/downloads'/folder
        download_folder.mkdir(parents=True, exist_ok=True)
        installer_name = stem+'_Package_Setup.exe'
        guide_name = 'CarillonBells_9_0_'+('Quick_Start' if kind == 'New_Install' else 'Database_Migration')+'_Guide.pdf'
        before = ('Use an empty USB drive for New Install.' if kind == 'New_Install' else
                  'Back up the complete existing installation and close the old bell program first. Choose the existing USB folder beside databases.')
        instructions = f'''Westminster Chimes and Carillon Bells 9.0

{kind.replace('_', ' ')} - {'32' if runtime == 'win-x86' else '64'}-bit Windows

1. Right-click this downloaded ZIP and choose Extract All. Extract it to a folder
   on your computer, such as Downloads. Do not run Setup from inside the ZIP.
2. In the extracted folder, double-click {installer_name}.
3. Click Continue. Read the license and accept it if you agree.
4. {before}
   Select your USB in Install folder. For example, E:\\V9 uses a V9 folder on E:
   and E:\\ uses that drive directly. Use your own drive letter and folder.
5. Click Install and wait for Install completed. Then click Verify and wait for
   Verify completed. Close Setup.
6. On the USB, double-click CarillonBells.exe to start the bell program.

Setup already contains the program files. No second ZIP download is needed.
For illustrated instructions, double-click {guide_name}.

ZIP packaging does not remove Windows security warnings. If Windows blocks a
file, check its source and contact the supplier or administrator for help.
Do not disable Windows protection to install this application.
'''
        download = download_folder/(stem+'_Package_Setup.zip')
        pending = download.with_suffix('.pending.zip')
        with zipfile.ZipFile(pending, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
            archive.write(installers[0], installer_name)
            archive.write(root/'docs/pdf'/guide_name, guide_name)
            archive.writestr('START_HERE.txt', instructions)
        with zipfile.ZipFile(pending) as archive:
            assert archive.testzip() is None
            assert archive.read(installer_name) == installers[0].read_bytes()
        pending.replace(download)
        print('Built public installer ZIP:', download.relative_to(root), flush=True)
