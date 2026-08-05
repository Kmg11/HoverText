; Inno Setup script for Hover Text (HoverTextWin).
; Build with:  dotnet publish ... -o bin/Release/publish
;              & "...\ISCC.exe" installer\installer.iss [/DAppVersion=x.y.z]
;
; Requires the published output in bin\Release\publish (single-file exe).

#define AppName "Hover Text"
#define AppExe "HoverTextWin.exe"
#define AppPublisher "HoverTextWin"
#define AppId "{{B5F0C7E4-4C0F-4F6A-9A6E-8D0B1E4F2C31}"
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\bin\Release\publish"
#endif
#ifndef OutputDir
  #define OutputDir "..\bin\Release\installer"
#endif

[Setup]
AppId={#AppId}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={userpf}\HoverText
DefaultGroupName=Hover Text
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=HoverTextSetup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
SetupIconFile=..\assets\app.ico
UninstallDisplayIcon={app}\{#AppExe},0
UninstallDisplayName=Hover Text
CloseApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "startup"; Description: "Launch Hover Text when Windows starts"; GroupDescription: "Startup:"

[Files]
Source: "{#PublishDir}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\*.pdb"; DestDir: "{app}"; Flags: ignoreversion skipifsourcedoesntexist

[Icons]
Name: "{autoprograms}\Hover Text"; Filename: "{app}\{#AppExe}"; IconFilename: "{app}\{#AppExe},0"
Name: "{autoprograms}\Hover Text\Uninstall Hover Text"; Filename: "{uninstallexe}"; IconFilename: "{app}\{#AppExe},0"

[Registry]
; Optional launch-with-Windows, chosen at install time. The in-app Options
; screen can toggle the same value later.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "HoverTextWin"; ValueData: """{app}\{#AppExe}"""; Flags: uninsdeletevalue; Tasks: startup

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // Drop the launch-at-startup value even if it was set from inside the app
  // (the installer task only cleans up when it created the value itself).
  if CurUninstallStep = usUninstall then
    RegDeleteValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Run', 'HoverTextWin');
end;

