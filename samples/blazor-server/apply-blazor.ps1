param(
    [string]$KubectlExe = "",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
$KubectlExe = Resolve-SampleKubectlExe -KubectlExe $KubectlExe
$kubectlArgs = Get-SampleKubectlArgs -KubeConfig $KubeConfig

$yaml = Join-Path $PSScriptRoot "k8s\blazor-server.yaml"
& $KubectlExe @kubectlArgs apply -f $yaml

Write-Host ""
Write-Host "Blazor Server sample applied:" -ForegroundColor Green
Write-Host "  $yaml"
Write-Host ""
Write-Host "Check pods:" -ForegroundColor Cyan
Write-Host ('  kubectl --kubeconfig "' + $env:KUBECONFIG + '" get pods')
Write-Host ""
Write-Host "Open browser via port-forward:" -ForegroundColor Cyan
Write-Host "  .\port-forward-blazor.ps1"
