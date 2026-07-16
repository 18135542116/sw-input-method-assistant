#define MyAppName "SW 输入法助手"
#define MyAppVersion "1.0.4"
#define MyAppPublisher "白云"
#define MyAppExeName "SwInputMethodAssistant.exe"

[Setup]
AppId={{D61DB38B-13D8-4D23-81FC-2347832C75B3}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\SW输入法助手
DefaultGroupName={#MyAppName}
UninstallDisplayName={#MyAppName}
OutputDir=installer-output
OutputBaseFilename=SW输入法助手安装包_v1.0.4
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest

[Tasks]
Name: "autostart"; Description: "安装后自动开机启动（推荐）"; GroupDescription: "附加选项："; Flags: checkedonce
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加选项："

[Files]
Source: "publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\SW 输入法助手"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\SW 输入法助手"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "SwInputMethodAssistant"; ValueData: """{app}\{#MyAppExeName}"" --background"; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "立即启动 SW 输入法助手"; Flags: nowait postinstall skipifsilent
