@echo off
setlocal
if exist "%~dp0CarillonBells.exe" (
  start "Carillon Bells" "%~dp0CarillonBells.exe" --evaluation
) else (
  set "DOTNET_ROOT=%~dp0tools\dotnet"
  start "Carillon Bells" "%~dp0bin\Release\CarillonBells.exe" --evaluation
)
