param(
    [string]$KubectlExe = "kubectl",
    [string]$LocalPort = "8080"
)

$ErrorActionPreference = "Stop"

Write-Host "Open this URL in your browser:" -ForegroundColor Cyan
Write-Host "  http://localhost:$LocalPort"
Write-Host ""
Write-Host "Press Ctrl+C to stop port-forward."
Write-Host ""

& $KubectlExe port-forward svc/sample-blazor-server ${LocalPort}:80
