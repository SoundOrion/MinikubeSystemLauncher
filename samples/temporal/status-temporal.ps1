param(
    [string]$Namespace = "temporal"
)

$ErrorActionPreference = "Stop"

Write-Host "Pods" -ForegroundColor Cyan
kubectl get pods -n $Namespace -o wide

Write-Host ""
Write-Host "Services" -ForegroundColor Cyan
kubectl get svc -n $Namespace

Write-Host ""
Write-Host "Helm releases" -ForegroundColor Cyan
helm list -n $Namespace
