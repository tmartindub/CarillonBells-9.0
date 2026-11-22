from pathlib import Path
import zipfile, json, hashlib
root=Path(__file__).resolve().parents[1]
results=[]
for path in sorted((root/'export').glob('CarillonBells_9_0_Evaluation_*.zip')):
    with zipfile.ZipFile(path) as archive:
        names=archive.namelist()
        assert all(not name.lower().endswith(('.db','.mp3','.wav','.dmp','.db-wal','.db-shm')) for name in names)
        assert all(not any(part.lower() in ('logs','songs','old_songs','databases') for part in Path(name).parts) for name in names)
        manifest=json.loads(archive.read('package-manifest.json').decode('utf-8-sig'))
        for item in manifest['files']:
            content=archive.read(item['path'])
            assert len(content)==item['bytes'] and hashlib.sha256(content).hexdigest().upper()==item['sha256'].upper(),item['path']
        assert 'CarillonBells.exe' in names and 'help/index.html' in names
        assert any(name.endswith('User_Guide.docx') for name in names)
        assert any(name.endswith('Engineering_Guide.pdf') for name in names)
        results.append({'file':path.name,'bytes':path.stat().st_size,'sha256':hashlib.sha256(path.read_bytes()).hexdigest().upper(),'sourceCommit':manifest['sourceCommit'],'runtime':manifest['runtime'],'fileCount':len(names),'verification':'PASS all manifest hashes and no private database/music/log files'})
(root/'source distributions').mkdir(exist_ok=True)
(root/'source distributions/release-manifest.json').write_text(json.dumps({'version':'9.0.0-evaluation','packages':results,'productionPromotionApproved':False},indent=2),encoding='utf-8')
print('PASS',len(results),'evaluation packages: all manifest hashes, required guides/help, and private-data exclusions.')
