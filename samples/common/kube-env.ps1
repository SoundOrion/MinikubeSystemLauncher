function Get-SampleWorkspaceRoot {
    # samples/common -> samples -> project root -> workspace root
    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\..\.."))
}

function Get-SampleProjectRoot {
    # samples/common -> samples -> project root
    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
}

function Get-SampleAppSetting {
    param(
        [string]$ConfigPath,
        [string]$Key
    )

    if (-not (Test-Path $ConfigPath)) {
        return ""
    }

    try {
        [xml]$xml = Get-Content -LiteralPath $ConfigPath
        $node = $xml.configuration.appSettings.add | Where-Object { $_.key -eq $Key } | Select-Object -First 1
        if ($null -eq $node) { return "" }
        return [string]$node.value
    }
    catch {
        return ""
    }
}

function Resolve-SampleKubeConfig {
    param(
        [string]$KubeConfig
    )

    if ([string]::IsNullOrWhiteSpace($KubeConfig)) {
        $KubeConfig = Join-Path (Get-SampleWorkspaceRoot) ".kube\config"
    }

    return [System.IO.Path]::GetFullPath($KubeConfig)
}

function Use-SampleKubeConfig {
    param(
        [string]$KubeConfig
    )

    $resolved = Resolve-SampleKubeConfig -KubeConfig $KubeConfig

    if (Test-Path $resolved) {
        $env:KUBECONFIG = $resolved
        Write-Host "KUBECONFIG:" -ForegroundColor DarkGray
        Write-Host "  $resolved" -ForegroundColor DarkGray
        Write-Host ""
        return $resolved
    }

    Write-Host "KUBECONFIG が見つかりません:" -ForegroundColor Yellow
    Write-Host "  $resolved" -ForegroundColor Yellow
    Write-Host "現在の kubectl context を使用します。" -ForegroundColor Yellow
    Write-Host ""
    return ""
}

function Resolve-SampleToolExe {
    param(
        [string]$ToolName,
        [string]$ProvidedExe,
        [string[]]$WorkspaceCandidates,
        [string]$ConfigPath,
        [string]$ConfigKey,
        [string]$OverrideParameterName
    )

    $candidates = New-Object System.Collections.Generic.List[string]

    $providedLooksLikePath = $false
    if (-not [string]::IsNullOrWhiteSpace($ProvidedExe)) {
        $providedLooksLikePath = [System.IO.Path]::IsPathRooted($ProvidedExe) -or $ProvidedExe.Contains('\') -or $ProvidedExe.Contains('/')
        if ($providedLooksLikePath) {
            $candidates.Add($ProvidedExe)
        }
    }

    foreach ($candidate in $WorkspaceCandidates) {
        if (-not [string]::IsNullOrWhiteSpace($candidate)) {
            $candidates.Add($candidate)
        }
    }

    $configured = Get-SampleAppSetting -ConfigPath $ConfigPath -Key $ConfigKey
    if (-not [string]::IsNullOrWhiteSpace($configured)) {
        $candidates.Add($configured)
    }

    foreach ($candidate in $candidates) {
        $full = [System.IO.Path]::GetFullPath($candidate)
        if (Test-Path $full) {
            Write-Host "${ToolName}:" -ForegroundColor DarkGray
            Write-Host "  $full" -ForegroundColor DarkGray
            Write-Host ""
            return $full
        }
    }

    $commandName = if ([string]::IsNullOrWhiteSpace($ProvidedExe) -or $providedLooksLikePath) { $ToolName } else { $ProvidedExe }
    $command = Get-Command $commandName -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        Write-Host "${ToolName}:" -ForegroundColor DarkGray
        Write-Host "  $($command.Source)" -ForegroundColor DarkGray
        Write-Host ""
        return $command.Source
    }

    $message = "$ToolName.exe が見つかりません。PATHに通すか、-$OverrideParameterName でフルパスを指定してください。"
    if ($WorkspaceCandidates.Count -gt 0) {
        $message += "`n確認した候補:`n  " + (($WorkspaceCandidates | ForEach-Object { [System.IO.Path]::GetFullPath($_) }) -join "`n  ")
    }
    throw $message
}

function Resolve-SampleMinikubeExe {
    param(
        [string]$MinikubeExe
    )

    $workspace = Get-SampleWorkspaceRoot
    $project = Get-SampleProjectRoot
    $config = Join-Path $project "src\SystemMinikubeHost\App.config"

    return Resolve-SampleToolExe `
        -ToolName "minikube" `
        -ProvidedExe $MinikubeExe `
        -WorkspaceCandidates @(Join-Path $workspace "tools\minikube\minikube.exe") `
        -ConfigPath $config `
        -ConfigKey "MinikubeExePath" `
        -OverrideParameterName "MinikubeExe"
}

function Resolve-SampleKubectlExe {
    param(
        [string]$KubectlExe
    )

    $workspace = Get-SampleWorkspaceRoot
    $project = Get-SampleProjectRoot
    $config = Join-Path $project "src\UserKubeClient\App.config"

    return Resolve-SampleToolExe `
        -ToolName "kubectl" `
        -ProvidedExe $KubectlExe `
        -WorkspaceCandidates @(Join-Path $workspace "tools\kubectl\kubectl.exe") `
        -ConfigPath $config `
        -ConfigKey "KubectlExePath" `
        -OverrideParameterName "KubectlExe"
}

function Resolve-SampleHelmExe {
    param(
        [string]$HelmExe
    )

    $workspace = Get-SampleWorkspaceRoot
    $project = Get-SampleProjectRoot
    $config = Join-Path $project "src\UserKubeClient\App.config"

    return Resolve-SampleToolExe `
        -ToolName "helm" `
        -ProvidedExe $HelmExe `
        -WorkspaceCandidates @(
            (Join-Path $workspace "tools\helm\helm.exe"),
            (Join-Path $workspace "tools\helm\windows-amd64\helm.exe")
        ) `
        -ConfigPath $config `
        -ConfigKey "HelmExePath" `
        -OverrideParameterName "HelmExe"
}

function Get-SampleKubectlArgs {
    param(
        [string]$KubeConfig
    )

    $resolved = Use-SampleKubeConfig -KubeConfig $KubeConfig
    if ([string]::IsNullOrWhiteSpace($resolved)) {
        return @()
    }

    return @("--kubeconfig", $resolved)
}

function Get-SampleHelmArgs {
    param(
        [string]$KubeConfig
    )

    $resolved = Use-SampleKubeConfig -KubeConfig $KubeConfig
    if ([string]::IsNullOrWhiteSpace($resolved)) {
        return @()
    }

    return @("--kubeconfig", $resolved)
}
