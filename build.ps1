$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$out = Join-Path $root "out"

$hostProject = Join-Path $root "src\SystemMinikubeHost\SystemMinikubeHost.csproj"
$userProject = Join-Path $root "src\UserKubeClient\UserKubeClient.csproj"

if (Test-Path $out) {
    Remove-Item $out -Recurse -Force
}
New-Item -ItemType Directory -Force $out | Out-Null

Write-Host "Building launcher projects..."
Write-Host "  $hostProject"
Write-Host "  $userProject"

# SDK-style net48 projects can usually be built with dotnet build on Windows when the .NET Framework 4.8 targeting pack is installed.
dotnet build $hostProject -c Release
dotnet build $userProject -c Release

$hostSrc = Join-Path $root "src\SystemMinikubeHost\bin\Release\net48"
$userSrc = Join-Path $root "src\UserKubeClient\bin\Release\net48"

Copy-Item $hostSrc (Join-Path $out "SystemMinikubeHost") -Recurse
Copy-Item $userSrc (Join-Path $out "UserKubeClient") -Recurse

Write-Host ""
Write-Host "Build output:"
Write-Host "  $out\SystemMinikubeHost\SystemMinikubeHost.exe"
Write-Host "  $out\UserKubeClient\UserKubeClient.exe"
Write-Host ""
Write-Host "ConsoleJobSample, SampleBlazorServer, and TemporalWorkerSample are included in the solution for Visual Studio."
Write-Host "Use samples\console-job scripts for Job image tests."
Write-Host "Use samples\blazor-server scripts for Blazor Server image tests."
Write-Host "Use samples\temporal scripts for Temporal Server / Worker tests."
