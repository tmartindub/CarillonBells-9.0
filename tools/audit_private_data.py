"""Compare staged/source text with secret reference values without revealing them."""
from pathlib import Path
import sqlite3,sys,zipfile
root=Path(__file__).resolve().parents[1]
baseline=Path(r'C:\Downloads\FMX2CSharp-data\databases\carillon.db')
with sqlite3.connect(baseline.as_uri()+'?mode=ro',uri=True) as connection:
    private=connection.execute('SELECT email_password,email_user,email_recipients,test_recipient,morning_report_recipients FROM PL_SETTINGS WHERE settings_id=1').fetchone()
secrets=[str(value).encode() for value in private if value and len(str(value))>=6]
excluded={'.git','bin','dcu','export','private','__pycache__','dotnet','.dotnet-home','.nuget','guide-qa'}
violations=[]
for path in root.rglob('*'):
    if not path.is_file() or any(part in excluded for part in path.relative_to(root).parts):continue
    data=path.read_bytes()
    if path.suffix.lower()=='.docx':
        with zipfile.ZipFile(path) as archive:data=b'\n'.join(archive.read(name) for name in archive.namelist() if name.endswith('.xml'))
    if any(secret in data for secret in secrets):violations.append(str(path.relative_to(root)))
    if path.suffix.lower() in ('.db','.mp3','.wav','.dmp'):violations.append(str(path.relative_to(root)))
if violations:
    print('PRIVATE DATA AUDIT FAILED in these files only:',*violations,sep='\n');sys.exit(1)
print('PASS Private reference credentials/recipient values absent from publishable source and documents.')
