$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$sln = Join-Path $root "MinikubeSystemLauncher.sln"
$out = Join-Path $root "out"

if (Test-Path $out) {
    Remove-Item $out -Recurse -Force
}
New-Item -ItemType Directory -Force $out | Out-Null

Write-Host "Building .NET Framework 4.8 solution..."
Write-Host "Solution: $sln"

# SDK-style net48 projects can usually be built with dotnet build on Windows when the .NET Framework 4.8 targeting pack is installed.
dotnet build $sln -c Release

$hostSrc = Join-Path $root "src\SystemMinikubeHost\bin\Release\net48"
$userSrc = Join-Path $root "src\UserKubeClient\bin\Release\net48"

Copy-Item $hostSrc (Join-Path $out "SystemMinikubeHost") -Recurse
Copy-Item $userSrc (Join-Path $out "UserKubeClient") -Recurse

Write-Host ""
Write-Host "Build output:"
Write-Host "  $out\SystemMinikubeHost\SystemMinikubeHost.exe"
Write-Host "  $out\UserKubeClient\UserKubeClient.exe"
