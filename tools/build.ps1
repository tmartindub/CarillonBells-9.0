param([ValidateSet('Debug','Release')][string]$Configuration='Release',[ValidateSet('','win-x86','win-x64')][string]$Runtime='',[switch]$Publish)
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
    if($Publish) {
        if(-not $Runtime){throw 'Specify win-x86 or win-x64 when publishing.'}
        & $dotnet publish CarillonBells.csproj -c $Configuration -r $Runtime --self-contained true -o "export\staging-$Runtime" -p:DebugType=None
    } elseif($Runtime) { & $dotnet build CarillonBells.csproj -c $Configuration -r $Runtime }
    else { & $dotnet build CarillonBells.csproj -c $Configuration }
    if($LASTEXITCODE -ne 0){throw "Build failed with exit code $LASTEXITCODE"}
} finally {Pop-Location}
