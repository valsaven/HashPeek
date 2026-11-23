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

Download `ServerRegistrationManager.zip` from the [SharpShell v2.7.2.0 release](https://github.com/dwmkerr/sharpshell/releases/tag/v2.7.2.0) (or install the `ServerRegistrationManager` NuGet package) and extract it somewhere, for example `C:\Tools\SharpShell`. SharpShell docs call this tool `srm`, but the actual executable is `ServerRegistrationManager.exe`. It requires administrator rights, so open PowerShell **as Administrator**.

Register the DLL with the bitness of your Explorer process:

```powershell
& 'C:\Tools\SharpShell\ServerRegistrationManager.exe' install 'G:\dev\HashPeek\HashPeek\bin\Release\net48\HashPeek.dll' -codebase -os64
```

Restart Explorer (or sign out) afterwards. `-codebase` means the DLL must stay at that path; do not move or delete it while registered. For 32-bit Explorer use `-os32`.

To uninstall:

```powershell
& 'C:\Tools\SharpShell\ServerRegistrationManager.exe' uninstall 'G:\dev\HashPeek\HashPeek\bin\Release\net48\HashPeek.dll' -os64
```

Then restart Explorer again.

## Usage

Select one file, open **Properties**, and switch to the **Hashes** tab. The page shows `Calculating...` with progress until all hashes are ready, and each row has a **Copy** button. If calculation fails, the error appears at the top of the page.

The tab only appears for a single selected file, not for folders or multiple selections.

## License

GPL-3.0. See [LICENSE](LICENSE).
