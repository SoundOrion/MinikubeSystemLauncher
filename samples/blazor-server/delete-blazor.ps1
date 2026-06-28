param(
    [string]$KubectlExe = "kubectl"
)

$ErrorActionPreference = "Stop"

$yaml = Join-Path $PSScriptRoot "k8s\blazor-server.yaml"
& $KubectlExe delete -f $yaml
