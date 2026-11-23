param([ValidateSet('Debug','Release')][string]$Configuration='Release',[ValidateSet('','win-x86','win-x64')][string]$Runtime='',[switch]$Publish,[string]$PortableRoot='')
$ErrorActionPreference='Stop'
$projectRoot=Split-Path -Parent $PSScriptRoot
$dotnet=Join-Path $PSScriptRoot 'dotnet\dotnet.exe'
if(-not(Test-Path -LiteralPath $dotnet)) {$dotnet='C:\Program Files\dotnet\dotnet.exe'}
$env:DOTNET_CLI_HOME=Join-Path $PSScriptRoot '.dotnet-home'
$env:NUGET_PACKAGES=Join-Path $PSScriptRoot '.nuget'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
$env:DOTNET_GENERATE_ASPNET_CERTIFICATE='false'
Push-Location $projectRoot
try {
    if($PortableRoot -and (-not $Publish -or -not $Runtime)){throw 'PortableRoot requires -Publish and a runtime.'}
    if($Publish) {
        if(-not $Runtime){throw 'Specify win-x86 or win-x64 when publishing.'}
        & $dotnet publish CarillonBells.csproj -c $Configuration -r $Runtime --self-contained true -o "export\staging-$Runtime" -p:DebugType=None
        if($LASTEXITCODE -ne 0){throw "Carillon Bells publish failed with exit code $LASTEXITCODE"}
        & $dotnet publish tools\CarillonMigration\CarillonMigration.csproj -c $Configuration -r $Runtime --self-contained true -o "export\staging-$Runtime" -p:DebugType=None
    } elseif($Runtime) { & $dotnet build tools\CarillonMigration\CarillonMigration.csproj -c $Configuration -r $Runtime }
    else { & $dotnet build CarillonBells.sln -c $Configuration }
    if($LASTEXITCODE -ne 0){throw "Build failed with exit code $LASTEXITCODE"}
    if($Publish) {
        $stage=Join-Path $projectRoot "export\staging-$Runtime"
        Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md'),(Join-Path $projectRoot 'Launch-Carillon-Evaluation.cmd') -Destination $stage -Force
        foreach($guideFolder in @(@('guides','*.docx'),@('pdf','*.pdf'))) {
            $relative=Join-Path 'docs' $guideFolder[0]
            $guideStage=Join-Path $stage $relative
            New-Item -ItemType Directory -Path $guideStage -Force | Out-Null
            Get-ChildItem -LiteralPath (Join-Path $projectRoot $relative) -Filter $guideFolder[1] -File |
                Copy-Item -Destination $guideStage -Force
        }
    }
    if($PortableRoot) {
        $portable=Get-Item -LiteralPath $PortableRoot
        if(-not $portable.PSIsContainer -or -not(Test-Path -LiteralPath (Join-Path $portable.FullName 'databases\carillon.db')) -or -not(Test-Path -LiteralPath (Join-Path $portable.FullName 'CarillonBells.exe'))){throw 'PortableRoot must be an initialized Carillon v9 installation with its own database.'}
        foreach($destination in @((Join-Path $projectRoot "bin\$Configuration"),$portable.FullName)) {
            foreach($file in Get-ChildItem -LiteralPath $stage -File -Recurse) {
                $relative=$file.FullName.Substring($stage.Length+1)
                if($relative -match '^(databases|data|logs|Music|Songs|Random_songs|Random_songs-2)\\' -or $file.Name -match '\.lock$|regression-results\.txt$|package-manifest\.json$'){continue}
                $target=Join-Path $destination $relative
                New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
                Copy-Item -LiteralPath $file.FullName -Destination $target -Force
                if((Get-FileHash -LiteralPath $target).Hash -ne (Get-FileHash -LiteralPath $file.FullName).Hash){throw "Deployment hash mismatch: $relative"}
            }
        }
        Write-Host "Portable application files updated and hash-verified: $($portable.FullName). Database and music preserved."
    }
} finally {Pop-Location}
