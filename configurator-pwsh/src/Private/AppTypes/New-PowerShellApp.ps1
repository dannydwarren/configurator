function New-PowerShellApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp,

        [Parameter(Mandatory)]
        [string]$ManifestDirectory
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    $appDir = Join-Path $ManifestDirectory 'apps' $appId
    $installPath = Join-Path $appDir 'install.ps1'
    $upgradePath = Join-Path $appDir 'upgrade.ps1'
    $verificationPath = Join-Path $appDir 'verification.ps1'

    if (-not (Test-Path $installPath)) {
        return $null
    }

    $installScript = ". `"$installPath`""
    $upgradeScript = if (Test-Path $upgradePath) { ". `"$upgradePath`"" } else { $null }
    $verificationScript = if (Test-Path $verificationPath) { ". `"$verificationPath`"" } else { $null }

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'powerShell'
        Environments       = $environments
        InstallScript      = $installScript
        VerificationScript = $verificationScript
        UpgradeScript      = $upgradeScript
        InstallArgs        = $null
        PreventUpgrade     = $false
        Configuration      = $null
        IsDownloadApp      = $false
        Downloader         = $null
        DownloaderArgs     = $null
        DownloadedFilePath = $null
    }
}
