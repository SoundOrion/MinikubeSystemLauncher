param(
    [string]$Namespace = "temporal",
    [int]$LocalPort = 7233,
    [string]$KubectlExe = "",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
$KubectlExe = Resolve-SampleKubectlExe -KubectlExe $KubectlExe
$kubectlArgs = Get-SampleKubectlArgs -KubeConfig $KubeConfig

Write-Host "Temporal frontend gRPC:" -ForegroundColor Cyan
Write-Host "  localhost:$LocalPort"
Write-Host ""
Write-Host "Stop with Ctrl+C."
& $KubectlExe @kubectlArgs port-forward -n $Namespace svc/temporal-frontend ${LocalPort}:7233
