# HashPeek

A Windows Explorer property sheet tab that shows file hashes. Built with SharpShell on .NET Framework 4.8.

Hashes are computed in one pass over the file, on a background thread:

| Algorithm | Notes |
| --- | --- |
| SHA-1 | Legacy checksums |
| SHA-256 | Primary checksum |
| SHA-512 | |
| MD5 | Legacy sum files |
| CRC32 | Printed big-endian, like `crc32` tools |
| xxHash64 | Seed 0, printed like `xxhsum` |
| XXH3_64 | Seed 0, printed like `xxhsum -H3` |
| BLAKE3 | Via the .NET Framework build of `SAIB.Blake2Fast` |

The `SAIB.Blake2Fast` package is a fork that adds BLAKE3 and still targets .NET Framework 4.6; the original `SauceControl.Blake2Fast` package does not provide BLAKE3 on .NET Framework. `System.IO.Hashing` is pinned to 8.0.0 on purpose: it has `XxHash3` and references the same `System.Memory`/`Unsafe` assembly versions as `SAIB.Blake2Fast`, so Explorer can load the extension without binding redirects. All hashes are verified against known vectors in the tests.

## Build

Requirements: .NET SDK 8 or newer and Windows. .NET Framework 4.8 reference assemblies are restored from NuGet, so a local targeting pack is not needed.

```powershell
dotnet build -c Release
dotnet test
```

The shell extension is `HashPeek\bin\Release\net48\HashPeek.dll`.

## Install

No third-party installer is needed. SharpShell has a `[ComRegisterFunction]`, so the built-in `regasm.exe` from .NET Framework also registers the Explorer property sheet handler. Open PowerShell **as Administrator** (writing HKCR requires it) and run:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" /codebase 'G:\dev\HashPeek\HashPeek\bin\Release\net48\HashPeek.dll'
```

`/codebase` means the DLL must stay at that path; do not move or delete it while registered. A warning about registering an unsigned assembly is expected and harmless for personal use. Restart Explorer (or sign out) afterwards.

To uninstall:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" /unregister 'G:\dev\HashPeek\HashPeek\bin\Release\net48\HashPeek.dll'
```

Then restart Explorer again. Use the `Framework\v4.0.30319` path instead if Explorer is 32-bit. The optional SharpShell `ServerRegistrationManager.exe` tool works too (`install <dll> -codebase -os64`), but is not required.

## Usage

Select one file, open **Properties**, and switch to the **Hashes** tab. The page shows `Calculating...` with progress until all hashes are ready. Each row has a small copy-icon button; hovering it shows `Copy to clipboard`. If calculation fails, the error appears at the top of the page.

The tab only appears for a single selected file, not for folders or multiple selections.

Explorer keeps the registered DLL loaded. Restart Explorer (or sign out) before rebuilding the extension, otherwise the build cannot replace `HashPeek.dll`.

## Package for users

NSIS 3 is enough; no other installer framework is needed. One command builds, signs and verifies everything:

```powershell
.\build.ps1
```

It compiles the solution, signs `HashPeek.dll` (so NSIS embeds an already-signed DLL), builds the installer, then signs and verifies both files. The signing identity is not committed — create `build.local.ps1` next to `build.ps1` once:

```powershell
$Thumbprint = '<your certificate SHA-1>'
$SignTool = 'signtool.exe'
```

That file is gitignored. You can also pass `-Thumbprint <sha1> -SignTool <path>` directly. The version fields on `HashPeek-Setup.exe` are set by the `VI...` lines at the top of `HashPeek.nsi`.

`HashPeek-Setup.exe` copies the DLLs to `C:\Program Files\HashPeek`, registers the COM server, and adds **HashPeek** to Settings → Apps → Installed apps. Uninstall from there. Both installer and uninstaller stop and restart Explorer automatically, so save anything open in File Explorer first. UAC elevation is required: Explorer writes to HKCR.

The script targets 64-bit Windows Explorer (`Framework64\regasm.exe`). For 32-bit Explorer, change `REGASM` in `HashPeek.nsi` to the `Framework\v4.0.30319` path.

## Remove the manual registration

If you registered the DLL by hand with `regasm`, remove it from an elevated PowerShell before switching to the installer:

```powershell
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\regasm.exe" /unregister 'G:\dev\HashPeek\HashPeek\bin\Release\net48\HashPeek.dll'
```

Restart Explorer afterwards. SharpShell's unregister callback removes the `ShellEx\PropertySheetHandlers\HashPropertySheet` key as well. Alternatively, just run the installer: it re-registers the same CLSID from the new location.

## License

GPL-3.0. See [LICENSE](LICENSE).
