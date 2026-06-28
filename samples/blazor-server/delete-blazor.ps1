param(
    [string]$KubectlExe = "kubectl",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
Set-SampleKubeConfig -KubeConfig $KubeConfig

$yaml = Join-Path $PSScriptRoot "k8s\blazor-server.yaml"
& $KubectlExe delete -f $yaml
