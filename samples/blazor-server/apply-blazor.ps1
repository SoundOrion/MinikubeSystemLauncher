param(
    [string]$KubectlExe = "kubectl"
)

$ErrorActionPreference = "Stop"

$yaml = Join-Path $PSScriptRoot "k8s\blazor-server.yaml"
& $KubectlExe apply -f $yaml

Write-Host ""
Write-Host "Blazor Server sample applied:" -ForegroundColor Green
Write-Host "  $yaml"
Write-Host ""
Write-Host "Check pods:" -ForegroundColor Cyan
Write-Host "  kubectl get pods"
Write-Host ""
Write-Host "Open browser via port-forward:" -ForegroundColor Cyan
Write-Host "  .\port-forward-blazor.ps1"
