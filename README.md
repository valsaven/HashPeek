# HashPeek

A Windows Explorer property sheet tab that shows file hashes. Built with SharpShell on .NET Framework 4.8.

Hashes are computed in one pass over the file, on a background thread:

| Algorithm | Notes |
| --- | --- |
| SHA-256 | Primary checksum |
| SHA-512 | |
| MD5 | Legacy sum files |
| CRC32 | Printed big-endian, like `crc32` tools |
| xxHash64 | Seed 0, printed like `xxhsum` |

BLAKE3 is intentionally not included: the maintained .NET package requires .NET 8, while SharpShell requires .NET Framework.

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

Select one file, open **Properties**, and switch to the **Hashes** tab. The page shows `Calculating...` with progress until all hashes are ready, and each row has a **Copy** button. If calculation fails, the error appears at the top of the page.

The tab only appears for a single selected file, not for folders or multiple selections.

## License

GPL-3.0. See [LICENSE](LICENSE).
