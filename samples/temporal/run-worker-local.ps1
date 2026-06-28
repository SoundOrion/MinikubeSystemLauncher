param(
    [string]$TemporalAddress = "localhost:7233",
    [string]$TemporalNamespace = "default",
    [string]$TaskQueue = "sample-task-queue"
)

$ErrorActionPreference = "Stop"

Push-Location $PSScriptRoot
try {
    $env:TEMPORAL_ADDRESS = $TemporalAddress
    $env:TEMPORAL_NAMESPACE = $TemporalNamespace
    $env:TEMPORAL_TASK_QUEUE = $TaskQueue

    Write-Host "Starting local Temporal worker" -ForegroundColor Cyan
    Write-Host "  TEMPORAL_ADDRESS=$env:TEMPORAL_ADDRESS"
    Write-Host "  TEMPORAL_NAMESPACE=$env:TEMPORAL_NAMESPACE"
    Write-Host "  TEMPORAL_TASK_QUEUE=$env:TEMPORAL_TASK_QUEUE"
    Write-Host ""
    Write-Host "Stop with Ctrl+C."

    dotnet run --project .\TemporalWorkerSample\TemporalWorkerSample.csproj -- worker
}
finally {
    Pop-Location
}
