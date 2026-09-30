#define AppName "WinRevive Control Center"
#define AppVersion "0.1.0"
#define AppPublisher "WinRevive"
#define AppExeName "WinRevive.ControlCenter.exe"

[Setup]
AppId={{E4D7AFB4-4AF1-4A26-94BF-1C7D7E4E2E0D}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\WinRevive
DefaultGroupName={#AppName}
OutputBaseFilename=WinRevive-Setup
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
DisableProgramGroupPage=yes

[Files]
Source: "..\src\WinRevive\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\WinRevive"
