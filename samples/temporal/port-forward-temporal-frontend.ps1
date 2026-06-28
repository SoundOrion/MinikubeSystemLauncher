param(
    [string]$Namespace = "temporal",
    [int]$LocalPort = 7233
)

$ErrorActionPreference = "Stop"

Write-Host "Temporal frontend gRPC:" -ForegroundColor Cyan
Write-Host "  localhost:$LocalPort"
Write-Host ""
Write-Host "Stop with Ctrl+C."
kubectl port-forward -n $Namespace svc/temporal-frontend ${LocalPort}:7233
