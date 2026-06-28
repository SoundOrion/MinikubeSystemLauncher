param(
    [string]$ImageTag = "sample-temporal-worker:dev",
    [string]$Profile = "minikube"
)

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    minikube image build -t $ImageTag .\TemporalWorkerSample --profile=$Profile

    Write-Host ""
    Write-Host "Worker image created in minikube:" -ForegroundColor Green
    Write-Host "  $ImageTag"
    Write-Host ""
    Write-Host "Apply optional worker deployment:" -ForegroundColor Cyan
    Write-Host "  kubectl apply -f .\k8s\worker-deployment.yaml"
}
finally {
    Pop-Location
}
