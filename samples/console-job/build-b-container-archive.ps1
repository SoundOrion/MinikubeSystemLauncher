param(
    [string]$Configuration = "Release",
    [string]$Os = "linux",
    [string]$Arch = "x64",
    [string]$ImageName = "sample-console-job",
    [string]$ImageTag = "dev"
)

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    $project = Join-Path $PSScriptRoot "ConsoleJobSample\ConsoleJobSample.csproj"
    $artifacts = Join-Path $PSScriptRoot "artifacts"
    $archive = Join-Path $artifacts "$ImageName-$ImageTag.tar.gz"

    New-Item -ItemType Directory -Force $artifacts | Out-Null

    dotnet publish $project `
        -c $Configuration `
        --os $Os `
        --arch $Arch `
        /t:PublishContainer `
        -p:ContainerRepository=$ImageName `
        -p:ContainerImageTag=$ImageTag `
        -p:ContainerArchiveOutputPath=$archive

    Write-Host ""
    Write-Host "Container archive created:" -ForegroundColor Green
    Write-Host "  $archive"
    Write-Host ""
    Write-Host "Load into minikube:" -ForegroundColor Cyan
    Write-Host "  minikube image load `"$archive`" --overwrite=true --profile=minikube"
}
finally {
    Pop-Location
}
