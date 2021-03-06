; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Audioframe"
#define MyAppVersion "1.0"
#define MyAppPublisher "Christoph Royer"
#define MyAppExeName "Audioframe.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{DBCEBB4D-F776-4BCD-926D-8872768C1CEA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=C:\Users\Christoph Royer\Desktop\Audioframe
OutputBaseFilename=AFinstall
SetupIconFile=C:\Users\Christoph Royer\Documents\Visual Studio 2017\Projects\Artsy Stuff\Audioframe\bin\Debug\af.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "C:\Users\Christoph Royer\Desktop\Audioframe\Audioframe.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Christoph Royer\Desktop\Audioframe\AxInterop.WMPLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Christoph Royer\Desktop\Audioframe\Bass.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Christoph Royer\Desktop\Audioframe\Bass.Net.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Christoph Royer\Desktop\Audioframe\Interop.WMPLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Christoph Royer\Desktop\Audioframe\NAudio.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\Christoph Royer\Desktop\Audioframe\NAudio.xml"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

