param([ValidateSet('win-x86','win-x64')][string]$Runtime,[string]$SourceCommit='uncommitted')
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$stage=Join-Path $projectRoot "export\staging-$Runtime"
foreach($executable in 'CarillonBells.exe','CarillonMigration.exe') {
    if(-not(Test-Path -LiteralPath (Join-Path $stage $executable))){throw 'Publish the application and migration wizard for this architecture first.'}
}
$forbidden=Get-ChildItem -LiteralPath $stage -Recurse -File | Where-Object {$_.Extension -in '.db','.mp3','.wav','.dmp' -or $_.Name -like '*.db-*' -or $_.FullName -match '\\(logs|Songs|old_Songs)\\'}
if($forbidden){throw 'Staging contains private/test data or music. Clean it before packaging.'}
Copy-Item -LiteralPath (Join-Path $projectRoot 'Launch-Carillon-Evaluation.cmd') -Destination $stage
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination $stage
New-Item -ItemType Directory -Path (Join-Path $stage 'docs\guides'),(Join-Path $stage 'docs\pdf') -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'docs\guides') -Filter '*.docx' -File | Copy-Item -Destination (Join-Path $stage 'docs\guides')
Get-ChildItem -LiteralPath (Join-Path $projectRoot 'docs\pdf') -Filter '*.pdf' -File | Copy-Item -Destination (Join-Path $stage 'docs\pdf')
$files=Get-ChildItem -LiteralPath $stage -File -Recurse | Where-Object {$_.FullName -ne (Join-Path $stage 'package-manifest.json')} | ForEach-Object {[ordered]@{path=$_.FullName.Substring($stage.Length+1).Replace('\','/');bytes=$_.Length;sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}}
$manifest=[ordered]@{product='Carillon Bells';version='9.0.0-evaluation';runtime=$Runtime;sourceCommit=$SourceCommit;selfContained=$true;singleFile=$false;files=@($files)}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $stage 'package-manifest.json') -Encoding utf8
$zip=Join-Path $projectRoot "export\CarillonBells_9_0_Evaluation_$Runtime.zip"
if(Test-Path -LiteralPath $zip){throw 'The package already exists. Use a new output name or explicitly remove the disposable prior package.'}
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
Get-FileHash -LiteralPath $zip -Algorithm SHA256
