param(
    [string]$Name = "Temporal",
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

    dotnet run --project .\TemporalWorkerSample\TemporalWorkerSample.csproj -- start $Name
}
finally {
    Pop-Location
}
