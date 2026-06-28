param(
    [string]$ImageTag = "sample-blazor-server:dev",
    [string]$MinikubeExe = "",
    [string]$Profile = "minikube"
)

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
$MinikubeExe = Resolve-SampleMinikubeExe -MinikubeExe $MinikubeExe

Push-Location $PSScriptRoot
try {
    $projectDir = Join-Path $PSScriptRoot "SampleBlazorServer"

    Write-Host "Build Blazor Server image in minikube:" -ForegroundColor Cyan
    Write-Host "  image: $ImageTag"
    Write-Host "  path : $projectDir"
    Write-Host ""

    & $MinikubeExe image build -t $ImageTag $projectDir --profile=$Profile

    Write-Host ""
    Write-Host "Container image built in minikube:" -ForegroundColor Green
    Write-Host "  $ImageTag"
    Write-Host ""
    Write-Host "Apply Kubernetes YAML:" -ForegroundColor Cyan
    Write-Host "  .\apply-blazor.ps1"
}
finally {
    Pop-Location
}
