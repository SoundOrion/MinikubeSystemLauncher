function Set-SampleKubeConfig {
    param(
        [string]$KubeConfig
    )

    if ([string]::IsNullOrWhiteSpace($KubeConfig)) {
        $KubeConfig = Join-Path $PSScriptRoot "..\..\..\.kube\config"
    }

    $KubeConfig = [System.IO.Path]::GetFullPath($KubeConfig)

    if (Test-Path $KubeConfig) {
        $env:KUBECONFIG = $KubeConfig
        Write-Host "KUBECONFIG:" -ForegroundColor DarkGray
        Write-Host "  $KubeConfig" -ForegroundColor DarkGray
        Write-Host ""
    }
    else {
        Write-Host "KUBECONFIG が見つかりません:" -ForegroundColor Yellow
        Write-Host "  $KubeConfig" -ForegroundColor Yellow
        Write-Host "現在の kubectl context を使用します。" -ForegroundColor Yellow
        Write-Host ""
    }
}
