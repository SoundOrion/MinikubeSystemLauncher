param(
    [string]$ImageTag = "sample-blazor-server:dev",
    [string]$MinikubeExe = "minikube",
    [string]$Profile = "minikube"
)

$ErrorActionPreference = "Stop"

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
    Write-Host "  kubectl apply -f `"$PSScriptRoot\k8s\blazor-server.yaml`""
}
finally {
    Pop-Location
}
