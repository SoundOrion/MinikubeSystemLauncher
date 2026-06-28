param(
    [string]$KubectlExe = "",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
$KubectlExe = Resolve-SampleKubectlExe -KubectlExe $KubectlExe
$kubectlArgs = Get-SampleKubectlArgs -KubeConfig $KubeConfig

$yaml = Join-Path $PSScriptRoot "k8s\blazor-server.yaml"
& $KubectlExe @kubectlArgs delete -f $yaml
