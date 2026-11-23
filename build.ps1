param(
    [string] $Thumbprint,
    [string] $SignTool = 'signtool.exe'
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

# Signing identity lives in an untracked local file, not in the repository.
$local = Join-Path $root 'build.local.ps1'
if (-not $Thumbprint -and (Test-Path $local)) { . $local }
if (-not $Thumbprint) { throw 'No signing thumbprint. Pass -Thumbprint or create build.local.ps1 (see README).' }
$dll = Join-Path $root 'HashPeek\bin\Release\net48\HashPeek.dll'
$setup = Join-Path $root 'HashPeek-Setup.exe'
$signArgs = @('sign', '/sha1', $Thumbprint, '/fd', 'sha256', '/tr', 'http://timestamp.digicert.com', '/td', 'sha256')

dotnet build (Join-Path $root 'HashPeek.sln') -c Release --nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

# Sign before packaging so NSIS embeds the already-signed DLL.
& $SignTool @signArgs $dll
if ($LASTEXITCODE -ne 0) { throw 'DLL signing failed' }

& makensis (Join-Path $root 'HashPeek.nsi')
if ($LASTEXITCODE -ne 0) { throw 'Installer build failed' }

& $SignTool @signArgs $setup
if ($LASTEXITCODE -ne 0) { throw 'Installer signing failed' }

& $SignTool verify /pa $dll
if ($LASTEXITCODE -ne 0) { throw 'DLL signature verification failed' }
& $SignTool verify /pa $setup
if ($LASTEXITCODE -ne 0) { throw 'Installer signature verification failed' }
