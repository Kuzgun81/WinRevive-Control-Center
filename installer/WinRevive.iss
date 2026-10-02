#define AppName "WinRevive Control Center"
#define AppVersion "0.4.2"
#define AppPublisher "WinRevive"
#define AppExeName "WinRevive.ControlCenter.exe"

[Setup]
AppId={{E4D7AFB4-4AF1-4A26-94BF-1C7D7E4E2E0D}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultGroupName={#AppName}
OutputBaseFilename=WinRevive-Setup
OutputDir=Output
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
UninstallDisplayIcon={app}\{#AppExeName}
AllowNoIcons=yes
SetupLogging=yes
UsePreviousAppDir=yes
DisableWelcomePage=no
DefaultDirName={localappdata}\Programs\WinRevive

[Files]
Source: "..\src\WinRevive\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Ek seçenekler:"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
