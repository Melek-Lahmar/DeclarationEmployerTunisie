#define MyAppName "Declaration Employeur Tunisie 2025"
#define MyAppVersion "0.1.0-foundation"
#define MyAppPublisher "Declaration Employer Tunisie"

[Setup]
AppId={{9F4E6D94-59DF-4A22-94B7-DET2025FOUNDATION}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Declaration Employeur Tunisie 2025
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=C:\DET2025_DEV\release\installer
OutputBaseFilename=DET2025_Foundation_Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Dirs]
Name: "{commonappdata}\DET2025\storage"
Name: "{commonappdata}\DET2025\logs"
Name: "{commonappdata}\DET2025\backups"

[Files]
Source: "C:\DET2025_DEV\release\desktop\*"; DestDir: "{app}\desktop"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "C:\DET2025_DEV\release\api\*"; DestDir: "{app}\api"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\desktop\DeclarationEmployer.Desktop.exe"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\desktop\DeclarationEmployer.Desktop.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Creer un raccourci bureau"; GroupDescription: "Raccourcis"

[Run]
Filename: "{app}\desktop\DeclarationEmployer.Desktop.exe"; Description: "Lancer {#MyAppName}"; Flags: nowait postinstall skipifsilent
