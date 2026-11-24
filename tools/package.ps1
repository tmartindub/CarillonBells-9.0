param(
    [ValidateSet('win-x86','win-x64')][string]$Runtime,
    [string]$SourceCommit='uncommitted',
    [switch]$SingleExe,
    [string]$MusicDirectory='',
    [string]$StarterDatabase='',
    [switch]$UsbUpgrade
)
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
if($SingleExe) {
    if(!$Runtime){throw 'Choose the Windows runtime for the English single-executable USB package.'}
    if($UsbUpgrade -and ($MusicDirectory -or $StarterDatabase)){
        throw 'The USB upgrade package uses the existing database and music. Omit StarterDatabase and MusicDirectory.'
    }
    $executable=Get-Item -LiteralPath (Join-Path $projectRoot "export\staging-single-$Runtime\CarillonBells.exe")
    $payload=@{'CarillonBells.exe'=$executable}
    $folders=@('Documentation/')
    foreach($format in @(@('docs\guides','*.docx'),@('docs\pdf','*.pdf'))) {
        $guideFolder=Join-Path $projectRoot $format[0]
        $guides=@(Get-ChildItem -LiteralPath $guideFolder -Filter $format[1] -File)
        if(!$guides.Count){throw "No $($format[1]) guides were found in $guideFolder."}
        foreach($guide in $guides){$payload["Documentation/$($guide.Name)"]=$guide}
    }

    $packageKind='Upgrade'
    if($UsbUpgrade) {
        $payload['CarillonMigration.exe']=Get-Item -LiteralPath (Join-Path $projectRoot "export\staging-migration-single-$Runtime\CarillonMigration.exe")
    } else {
        if(!$MusicDirectory -or !$StarterDatabase){throw 'Select the Music folder and a prepared starter database for a new USB installation.'}
        $packageKind='New_Install'
        $folders+=@('data/','databases/','Help/','logs/','Music/')
        $starter=Get-Item -LiteralPath $StarterDatabase
        if($starter.PSIsContainer -or $starter.Extension -ne '.db' -or !$starter.Length -or ($starter.Attributes -band [IO.FileAttributes]::ReparsePoint)){
            throw 'Select a prepared SQLite starter database file.'
        }
        foreach($suffix in '-wal','-shm','-journal'){
            if(Test-Path -LiteralPath ($starter.FullName+$suffix)){throw 'Close the starter database and use a consistent snapshot for packaging.'}
        }
        $payload['databases/carillon.db']=$starter

        $music=Get-Item -LiteralPath $MusicDirectory
        if(!$music.PSIsContainer -or $music.Name -ne 'Music' -or ($music.Attributes -band [IO.FileAttributes]::ReparsePoint)){
            throw 'Select the actual Music folder as the deployment audio source.'
        }
        $entries=@(Get-ChildItem -LiteralPath $music.FullName -Recurse -Force)
        if($entries | Where-Object {$_.Attributes -band [IO.FileAttributes]::ReparsePoint}){throw 'Music contains a linked file or folder.'}
        $audio=@($entries | Where-Object {!$_.PSIsContainer -and $_.Extension -in '.mp3','.wav'})
        if(!$audio.Count){throw 'The Music folder has no MP3 or WAV files.'}
        foreach($file in $audio){
            $relative=$file.FullName.Substring($music.FullName.Length+1).Replace('\','/')
            $payload["Music/$relative"]=$file
        }

        $helpFolder=Join-Path $projectRoot 'help'
        if(!(Test-Path -LiteralPath (Join-Path $helpFolder 'index.html') -PathType Leaf)){throw 'The current Help index was not found.'}
        foreach($file in Get-ChildItem -LiteralPath $helpFolder -File -Recurse){
            $relative=$file.FullName.Substring($helpFolder.Length+1).Replace('\','/')
            $payload["Help/$relative"]=$file
        }
    }

    $zip=Join-Path $projectRoot "export\CarillonBells_9_0_USB_${packageKind}_$Runtime.zip"
    if(Test-Path -LiteralPath $zip){throw "The USB package already exists: $zip"}
    $partial=Join-Path (Split-Path -Parent $zip) ('.carillon-package-'+[guid]::NewGuid().ToString('N')+'.zip')

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    try {
        $archive=[IO.Compression.ZipFile]::Open($partial,[IO.Compression.ZipArchiveMode]::Create)
        try {
            foreach($folder in $folders){$archive.CreateEntry($folder) | Out-Null}
            foreach($name in ($payload.Keys | Sort-Object)){
                [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$payload[$name].FullName,$name,[IO.Compression.CompressionLevel]::Optimal) | Out-Null
            }
        } finally { $archive.Dispose() }
        $archive=[IO.Compression.ZipFile]::OpenRead($partial)
        try {
            if($archive.Entries.Count -ne ($payload.Count+$folders.Count)){throw 'Unexpected number of package entries.'}
            foreach($entry in $archive.Entries){
                if($folders -ccontains $entry.FullName){
                    if($entry.Length -ne 0){throw "Unexpected contents in directory entry $($entry.FullName)"}
                    continue
                }
                if(!$payload.ContainsKey($entry.FullName)){throw "Unexpected package file $($entry.FullName)"}
                $stream=$entry.Open()
                try {
                    if((Get-FileHash -InputStream $stream).Hash -ne (Get-FileHash -LiteralPath $payload[$entry.FullName].FullName).Hash){
                        throw "Package verification failed for $($entry.FullName)"
                    }
                } finally { $stream.Dispose() }
            }
        } finally { $archive.Dispose() }
        [IO.File]::Move($partial,$zip)
    } finally {
        if(Test-Path -LiteralPath $partial){Remove-Item -LiteralPath $partial}
    }
    Write-Host "Verified English USB package: $packageKind."
    Get-FileHash -LiteralPath $zip
    return
}
if($MusicDirectory -or $StarterDatabase -or $UsbUpgrade){throw 'MusicDirectory, StarterDatabase, and UsbUpgrade require SingleExe.'}
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
$manifest=[ordered]@{product='Westminster Chimes and Carillon Bells';version='9.0.0-evaluation';runtime=$Runtime;sourceCommit=$SourceCommit;selfContained=$true;singleFile=$false;files=@($files)}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $stage 'package-manifest.json') -Encoding utf8
$zip=Join-Path $projectRoot "export\CarillonBells_9_0_Evaluation_$Runtime.zip"
if(Test-Path -LiteralPath $zip){throw 'The package already exists. Use a new output name or explicitly remove the disposable prior package.'}
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
Get-FileHash -LiteralPath $zip -Algorithm SHA256
