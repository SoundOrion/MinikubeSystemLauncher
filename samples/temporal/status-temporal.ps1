param(
    [string]$Namespace = "temporal",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
Set-SampleKubeConfig -KubeConfig $KubeConfig

Write-Host "Pods" -ForegroundColor Cyan
kubectl get pods -n $Namespace -o wide

Write-Host ""
Write-Host "Services" -ForegroundColor Cyan
kubectl get svc -n $Namespace

Write-Host ""
Write-Host "Helm releases" -ForegroundColor Cyan
helm list -n $Namespace
