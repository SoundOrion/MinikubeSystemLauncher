param(
    [string]$Configuration = "Release",
    [string]$Os = "linux",
    [string]$Arch = "x64"
)

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    New-Item -ItemType Directory -Force .\artifacts | Out-Null

    dotnet publish .\TemporalWorkerSample\TemporalWorkerSample.csproj `
        -c $Configuration `
        --os $Os `
        --arch $Arch `
        /t:PublishContainer `
        -p:ContainerRepository=sample-temporal-worker `
        -p:ContainerImageTag=dev `
        -p:ContainerArchiveOutputPath=.\artifacts\sample-temporal-worker-dev.tar.gz

    Write-Host ""
    Write-Host "Container archive created:" -ForegroundColor Green
    Write-Host "  $PSScriptRoot\artifacts\sample-temporal-worker-dev.tar.gz"
    Write-Host ""
    Write-Host "Load into minikube:" -ForegroundColor Cyan
    Write-Host "  .\load-worker-b-container-archive.ps1"
}
finally {
    Pop-Location
}
