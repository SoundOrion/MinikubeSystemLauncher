param(
    [string]$KubectlExe = "kubectl",
    [string]$LocalPort = "8080",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
Set-SampleKubeConfig -KubeConfig $KubeConfig

Write-Host "Open this URL in your browser:" -ForegroundColor Cyan
Write-Host "  http://localhost:$LocalPort"
Write-Host ""
Write-Host "Press Ctrl+C to stop port-forward."
Write-Host ""

& $KubectlExe port-forward svc/sample-blazor-server ${LocalPort}:80
