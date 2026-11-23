Unicode True
!include LogicLib.nsh
!include MUI2.nsh

Name "HashPeek"
OutFile "HashPeek-Setup.exe"
InstallDir "$PROGRAMFILES64\HashPeek"
RequestExecutionLevel admin

!define BUILD_OUT "HashPeek\bin\Release\net48"
!define REGASM "$WINDIR\Microsoft.NET\Framework64\v4.0.30319\regasm.exe"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\HashPeek"
!define CLSID "{e2c4a1f7-5b8d-4a3e-9c60-7d1f2b6a8c34}"

VIProductVersion "1.0.0.0"
VIFileVersion "1.0.0.0"
VIAddVersionKey "ProductName" "HashPeek"
VIAddVersionKey "ProductVersion" "1.0.0.0"
VIAddVersionKey "FileDescription" "HashPeek - Explorer hash tab"
VIAddVersionKey "FileVersion" "1.0.0.0"
VIAddVersionKey "LegalCopyright" "Copyright (c) 2026 Val Saven"
VIAddVersionKey "OriginalFilename" "HashPeek-Setup.exe"
VIAddVersionKey "Language" "English (United States)"

!macro StopExplorer
  ExecWait 'taskkill.exe /F /IM explorer.exe'
  Sleep 500
!macroend

!macro StartExplorer
  Exec '$WINDIR\explorer.exe'
!macroend

!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"

Section "Install"
  ; Explorer holds the registered DLLs open, so it must go down before copying.
  !insertmacro StopExplorer

  SetOutPath "$INSTDIR"
  File "${BUILD_OUT}\HashPeek.dll"
  File "${BUILD_OUT}\SharpShell.dll"
  File "${BUILD_OUT}\SauceControl.Blake2Fast.dll"
  File "${BUILD_OUT}\System.Buffers.dll"
  File "${BUILD_OUT}\System.IO.Hashing.dll"
  File "${BUILD_OUT}\System.Memory.dll"
  File "${BUILD_OUT}\System.Numerics.Vectors.dll"
  File "${BUILD_OUT}\System.Runtime.CompilerServices.Unsafe.dll"

  nsExec::Exec '"${REGASM}" /nologo /codebase "$INSTDIR\HashPeek.dll"'
  Pop $0
  ${If} $0 != 0
    !insertmacro StartExplorer
    MessageBox MB_ICONSTOP "Explorer registration failed (exit code $0). HashPeek was not installed."
    Abort
  ${EndIf}

  SetRegView 64
  WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayName" "HashPeek - Explorer hash tab"
  WriteRegStr HKLM "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr HKLM "${UNINSTALL_KEY}" "QuietUninstallString" '"$INSTDIR\uninstall.exe" /S'
  WriteRegStr HKLM "${UNINSTALL_KEY}" "Publisher" "HashPeek"
  WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoModify" 1
  WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoRepair" 1

  WriteUninstaller "$INSTDIR\uninstall.exe"
  !insertmacro StartExplorer
SectionEnd

Section "Uninstall"
  !insertmacro StopExplorer

  nsExec::Exec '"${REGASM}" /nologo /unregister "$INSTDIR\HashPeek.dll"'
  Pop $0
  ${If} $0 != 0
    MessageBox MB_ICONSTOP "Explorer unregistration failed (exit code $0). Registry entries may remain."
  ${EndIf}

  ; SharpShell writes these itself; remove any leftovers in case regasm missed them.
  DeleteRegKey HKCR "CLSID\${CLSID}"
  DeleteRegKey HKCR "HashPeek.HashPropertySheet"
  DeleteRegKey HKCR "*\ShellEx\PropertySheetHandlers\HashPropertySheet"

  SetRegView 64
  DeleteRegKey HKLM "${UNINSTALL_KEY}"

  Delete "$INSTDIR\HashPeek.dll"
  Delete "$INSTDIR\SharpShell.dll"
  Delete "$INSTDIR\SauceControl.Blake2Fast.dll"
  Delete "$INSTDIR\System.Buffers.dll"
  Delete "$INSTDIR\System.IO.Hashing.dll"
  Delete "$INSTDIR\System.Memory.dll"
  Delete "$INSTDIR\System.Numerics.Vectors.dll"
  Delete "$INSTDIR\System.Runtime.CompilerServices.Unsafe.dll"
  Delete "$INSTDIR\uninstall.exe"
  RMDir "$INSTDIR"

  !insertmacro StartExplorer
SectionEnd
