@echo off
setlocal
rem Builds the single-file publish and the Inno Setup installer.
rem Usage:  BuildInstaller.bat [version]     (default version 1.0.0)

cd /d "%~dp0\.."

set "ISCC="
for %%P in (
  "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
  "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
  "C:\Program Files\Inno Setup 6\ISCC.exe"
) do if not defined ISCC if exist %%P set "ISCC=%%~P"

if not defined ISCC (
  echo ERROR: ISCC.exe not found. Install Inno Setup with:
  echo   winget install JRSoftware.InnoSetup
  exit /b 1
)

set "VER=%~1"
if "%VER%"=="" set "VER=1.0.0"

echo Building single-file publish...
dotnet publish HoverTextWin.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin\Release\publish
if errorlevel 1 exit /b 1

echo Compiling installer v%VER%...
"%ISCC%" installer\installer.iss /DAppVersion=%VER%
if errorlevel 1 exit /b 1

echo.
echo Done: bin\Release\installer\HoverTextSetup-%VER%.exe
endlocal
