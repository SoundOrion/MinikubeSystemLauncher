param(
    [string]$Namespace = "temporal",
    [string]$PostgresRelease = "temporal-postgresql",
    [string]$TemporalRelease = "temporal",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
Set-SampleKubeConfig -KubeConfig $KubeConfig

Write-Host "Uninstalling Temporal Helm release" -ForegroundColor Cyan
helm uninstall $TemporalRelease --namespace $Namespace

Write-Host "Uninstalling PostgreSQL Helm release" -ForegroundColor Cyan
helm uninstall $PostgresRelease --namespace $Namespace

Write-Host ""
Write-Host "Helm releases were removed." -ForegroundColor Green
Write-Host "PostgreSQL PVC may remain so data can survive accidental uninstall." -ForegroundColor Yellow
Write-Host "To delete it manually, run:" -ForegroundColor Yellow
Write-Host "  kubectl delete pvc -n $Namespace -l app.kubernetes.io/instance=$PostgresRelease"
