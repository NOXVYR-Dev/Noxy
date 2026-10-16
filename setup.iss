#ifndef MyAppVersion
  #define MyAppVersion "0.2.0"
#endif

[Setup]
AppId={{E8D38CB2-1433-4AF8-BCC4-5B6B49D459A7}
AppName=Noxy
AppVersion={#MyAppVersion}
AppPublisher=NOXVYR-Dev
DefaultDirName={localappdata}\Programs\Noxy
DefaultGroupName=Noxy
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesInstallIn64BitMode=x64
OutputDir=installer-output
OutputBaseFilename=Noxy-Setup-x64
UninstallDisplayIcon={app}\Noxy.exe
CloseApplications=force
RestartApplications=no
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "autostart"; Description: "Noxy beim Windows-Anmelden automatisch starten"; GroupDescription: "Zusätzliche Optionen:"; Flags: checkedonce
Name: "desktopicon"; Description: "Desktop-Verknüpfung erstellen"; GroupDescription: "Zusätzliche Optionen:"

[Files]
Source: "publish\Noxy.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\Noxy"; Filename: "{app}\Noxy.exe"
Name: "{autodesktop}\Noxy"; Filename: "{app}\Noxy.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Noxy"; ValueData: """{app}\Noxy.exe"""; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\Noxy.exe"; Description: "Noxy starten"; Flags: postinstall nowait skipifsilent

