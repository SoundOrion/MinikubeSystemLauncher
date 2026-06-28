param(
    [string]$Namespace = "temporal",
    [int]$LocalPort = 8080
)

$ErrorActionPreference = "Stop"

Write-Host "Temporal Web UI:" -ForegroundColor Cyan
Write-Host "  http://localhost:$LocalPort"
Write-Host ""
Write-Host "Stop with Ctrl+C."
kubectl port-forward -n $Namespace svc/temporal-web ${LocalPort}:8080
