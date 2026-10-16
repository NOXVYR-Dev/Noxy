@echo off
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
 echo .NET 8 SDK fehlt. Bitte von https://dotnet.microsoft.com/download/dotnet/8.0 installieren.
 pause
 exit /b 1
)
dotnet publish Noxy.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableWindowsTargeting=true -o "%~dp0publish"
if errorlevel 1 (
 echo Build fehlgeschlagen.
 pause
 exit /b 1
)
echo Fertig: %~dp0publish\Noxy.exe
pause

