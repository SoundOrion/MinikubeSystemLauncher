param(
    [string]$ArchivePath = ".\artifacts\sample-temporal-worker-dev.tar.gz",
    [string]$Profile = "minikube"
)

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    if (-not (Test-Path $ArchivePath)) {
        throw "Container archive was not found: $ArchivePath"
    }

    minikube image load $ArchivePath --overwrite=true --profile=$Profile

    Write-Host ""
    Write-Host "Worker image loaded into minikube." -ForegroundColor Green
}
finally {
    Pop-Location
}
