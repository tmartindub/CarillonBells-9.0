"""Read-only reference inventory. Never emits private settings values."""
from pathlib import Path
import sqlite3, hashlib, json, re

root = Path(__file__).resolve().parents[1]
source = Path(r'C:\New Delphi Projects\Carillon8_1- Portable - FMX')
records = []
for path in sorted(source.rglob('*')):
    if any(p in ('__history', '__recovery', '.git') for p in path.parts):
        continue
    if path.suffix.lower() not in ('.pas', '.fmx', '.dpr', '.dproj'):
        continue
    content = path.read_text(encoding='utf-8-sig', errors='replace')
    records.append({'file': str(path.relative_to(source)), 'sha256': hashlib.sha256(path.read_bytes()).hexdigest(),
                    'lines': len(content.splitlines()),
                    'routines': re.findall(r'(?im)^(?:procedure|function|constructor|destructor)\s+([^;(\r\n]+)', content),
                    'controls': re.findall(r'(?im)^\s*(?:object|inherited)\s+(\w+)\s*:\s*(\w+)', content),
                    'events': re.findall(r'(?im)^\s*(On\w+|Interval|Enabled)\s*=\s*([^\r\n]+)', content) if path.suffix.lower()=='.fmx' else []})
(root/'docs/guides/source-inventory.json').write_text(json.dumps(records, indent=2), encoding='utf-8')
db = Path(r'C:\Downloads\FMX2CSharp-data\databases\carillon.db')
connection = sqlite3.connect(db.as_uri()+'?mode=ro', uri=True)
schema = []
for kind, name, sql in connection.execute("SELECT type,name,sql FROM sqlite_master WHERE sql IS NOT NULL ORDER BY type,name"):
    item = {'type':kind,'name':name,'sql':sql}
    if kind=='table':
        item['count']=connection.execute('SELECT COUNT(*) FROM "'+name.replace('"','""')+'"').fetchone()[0]
        item['types']={col[1]:connection.execute('SELECT typeof("'+col[1]+'"),count(*) FROM "'+name+'" GROUP BY 1').fetchall() for col in connection.execute('PRAGMA table_info("'+name+'")')}
    schema.append(item)
connection.close()
(root/'docs/guides/database-schema.json').write_text(json.dumps(schema,indent=2),encoding='utf-8')
clock = (source/'ClockUnit.fmx').read_text(encoding='utf-8-sig')
bitmap = re.search(r'PNG\s*=\s*\{([0-9A-Fa-f\s]+)\}',clock)
if not bitmap:
    bitmap = re.search(r'Bitmap\s*=\s*\{([0-9A-Fa-f\s]+)\}',clock)
if bitmap:
    data=bytes.fromhex(bitmap[1]); start=data.find(b'\x89PNG\r\n\x1a\n')
    if start>=0:
        (root/'images and icons/ClockFace.png').write_bytes(data[start:])
print('Inventoried',len(records),'source/form/project files; schema checked read-only; baseline SHA256',hashlib.sha256(db.read_bytes()).hexdigest())
print('Clock artwork extracted:',(root/'images and icons/ClockFace.png').exists())
for filename,output in [('CarillonSplash.fmx','Splash.png')]:
    form=(source/filename).read_text(encoding='utf-8-sig')
    match=re.search(r'PNG\s*=\s*\{([0-9A-Fa-f\s]+)\}',form)
    if match:
        (root/'images and icons'/output).write_bytes(bytes.fromhex(match[1]))
