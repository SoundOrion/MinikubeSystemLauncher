param(
    [string]$Namespace = "temporal",
    [string]$PostgresRelease = "temporal-postgresql",
    [string]$TemporalRelease = "temporal",
    [string]$KubeConfig = "")

$ErrorActionPreference = "Stop"

$commonKubeEnv = Join-Path $PSScriptRoot "..\common\kube-env.ps1"
. $commonKubeEnv
Set-SampleKubeConfig -KubeConfig $KubeConfig

Push-Location $PSScriptRoot
try {
    Write-Host "Creating namespace: $Namespace" -ForegroundColor Cyan
    kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f -

    Write-Host "Adding Helm repositories" -ForegroundColor Cyan
    helm repo add bitnami https://charts.bitnami.com/bitnami --force-update | Out-Host
    helm repo add temporal https://go.temporal.io/helm-charts/ --force-update | Out-Host
    helm repo update | Out-Host

    Write-Host "Installing PostgreSQL" -ForegroundColor Cyan
    helm upgrade --install $PostgresRelease bitnami/postgresql `
        --namespace $Namespace `
        --values .\values-postgresql.yaml

    Write-Host "Waiting for PostgreSQL" -ForegroundColor Cyan
    kubectl rollout status statefulset/temporal-postgresql `
        --namespace $Namespace `
        --timeout=300s

    Write-Host "Installing Temporal" -ForegroundColor Cyan
    helm upgrade --install $TemporalRelease temporal/temporal `
        --namespace $Namespace `
        --values .\values-temporal.yaml

    Write-Host "Waiting for Temporal frontend" -ForegroundColor Cyan
    kubectl rollout status deployment/temporal-frontend `
        --namespace $Namespace `
        --timeout=300s

    Write-Host "Waiting for Temporal Web UI" -ForegroundColor Cyan
    kubectl rollout status deployment/temporal-web `
        --namespace $Namespace `
        --timeout=300s

    Write-Host ""
    Write-Host "Temporal installed." -ForegroundColor Green
    Write-Host ""
    Write-Host "Show status:" -ForegroundColor Cyan
    Write-Host "  .\status-temporal.ps1"
    Write-Host ""
    Write-Host "Open Web UI:" -ForegroundColor Cyan
    Write-Host "  .\port-forward-temporal-ui.ps1"
    Write-Host "  http://localhost:8080"
    Write-Host ""
    Write-Host "Connect SDK / Worker from Windows:" -ForegroundColor Cyan
    Write-Host "  .\port-forward-temporal-frontend.ps1"
    Write-Host "  TEMPORAL_ADDRESS=localhost:7233"
}
finally {
    Pop-Location
}
