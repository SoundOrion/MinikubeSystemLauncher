param(
    [string]$ArchivePath = ".\artifacts\sample-temporal-worker-dev.tar.gz",
    [string]$MinikubeExe = "",
    [string]$Profile = "minikube"
)

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
$MinikubeExe = Resolve-SampleMinikubeExe -MinikubeExe $MinikubeExe

Push-Location $PSScriptRoot
try {
    if (-not (Test-Path $ArchivePath)) {
        throw "Container archive was not found: $ArchivePath"
    }

    & $MinikubeExe image load $ArchivePath --overwrite=true --profile=$Profile

    Write-Host ""
    Write-Host "Worker image loaded into minikube." -ForegroundColor Green
}
finally {
    Pop-Location
}
