param(
    [string]$Namespace = "temporal",
    [string]$PostgresRelease = "temporal-postgresql",
    [string]$TemporalRelease = "temporal",
    [string]$KubectlExe = "",
    [string]$HelmExe = "",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
$KubectlExe = Resolve-SampleKubectlExe -KubectlExe $KubectlExe
$HelmExe = Resolve-SampleHelmExe -HelmExe $HelmExe
$kubectlArgs = Get-SampleKubectlArgs -KubeConfig $KubeConfig
$helmArgs = if ([string]::IsNullOrWhiteSpace($env:KUBECONFIG)) { @() } else { @("--kubeconfig", $env:KUBECONFIG) }

Write-Host "Uninstalling Temporal Helm release" -ForegroundColor Cyan
& $HelmExe @helmArgs uninstall $TemporalRelease --namespace $Namespace

Write-Host "Uninstalling PostgreSQL Helm release" -ForegroundColor Cyan
& $HelmExe @helmArgs uninstall $PostgresRelease --namespace $Namespace

Write-Host ""
Write-Host "Helm releases were removed." -ForegroundColor Green
Write-Host "PostgreSQL PVC may remain so data can survive accidental uninstall." -ForegroundColor Yellow
Write-Host "To delete it manually, run:" -ForegroundColor Yellow
Write-Host ('  kubectl --kubeconfig "' + $env:KUBECONFIG + '" delete pvc -n ' + $Namespace + ' -l app.kubernetes.io/instance=' + $PostgresRelease)
