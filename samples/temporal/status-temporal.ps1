param(
    [string]$Namespace = "temporal",
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

Write-Host "Pods" -ForegroundColor Cyan
& $KubectlExe @kubectlArgs get pods -n $Namespace -o wide

Write-Host ""
Write-Host "Services" -ForegroundColor Cyan
& $KubectlExe @kubectlArgs get svc -n $Namespace

Write-Host ""
Write-Host "Helm releases" -ForegroundColor Cyan
& $HelmExe @helmArgs list -n $Namespace
