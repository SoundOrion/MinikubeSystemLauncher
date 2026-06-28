param(
    [string]$ArchivePath,
    [string]$MinikubeExe = "minikube",
    [string]$Profile = "minikube"
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ArchivePath)) {
    $ArchivePath = Join-Path $PSScriptRoot "artifacts\sample-console-job-dev.tar.gz"
}

if (-not (Test-Path $ArchivePath)) {
    throw "Container archive not found: $ArchivePath"
}

& $MinikubeExe image load $ArchivePath --overwrite=true --profile=$Profile

Write-Host ""
Write-Host "Container archive loaded into minikube:" -ForegroundColor Green
Write-Host "  $ArchivePath"
Write-Host ""
Write-Host "Apply Job:" -ForegroundColor Cyan
Write-Host "  kubectl apply -f `"$PSScriptRoot\job.yaml`""
