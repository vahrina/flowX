$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

foreach ($legacy in @('obj', 'bin')) {
    $lp = Join-Path $root $legacy
    if (Test-Path $lp) { Remove-Item $lp -Recurse -Force }
}

foreach ($p in @(
        Join-Path $root 'publish'
        Join-Path $root 'package'
        Join-Path $root 'release'
    )) {
    if (Test-Path $p) {
        Remove-Item $p -Recurse -Force
    }
}

$csproj = Join-Path $root 'flowx.csproj'
dotnet publish $csproj -c Release -o (Join-Path $root 'publish')
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$stage = Join-Path $root 'package\flowx'
New-Item -ItemType Directory -Path $stage -Force | Out-Null
Copy-Item (Join-Path $root 'publish\*') $stage -Recurse -Force

$releaseDir = Join-Path $root 'release'
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
$zip = Join-Path $releaseDir 'flowx.zip'
Compress-Archive -Path $stage -DestinationPath $zip -Force

Write-Host "ok: $zip"
