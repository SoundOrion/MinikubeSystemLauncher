param(
    [string]$Namespace = "temporal",
    [int]$LocalPort = 8080,
    [string]$KubectlExe = "",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
$KubectlExe = Resolve-SampleKubectlExe -KubectlExe $KubectlExe
$kubectlArgs = Get-SampleKubectlArgs -KubeConfig $KubeConfig

Write-Host "Temporal Web UI:" -ForegroundColor Cyan
Write-Host "  http://localhost:$LocalPort"
Write-Host ""
Write-Host "Stop with Ctrl+C."
& $KubectlExe @kubectlArgs port-forward -n $Namespace svc/temporal-web ${LocalPort}:8080
