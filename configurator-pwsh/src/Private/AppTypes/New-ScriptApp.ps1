function New-ScriptApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    $configuration = $null
    if ($null -ne $RawApp.configuration) {
        $configuration = $RawApp.configuration
    }

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'script'
        Environments       = $environments
        InstallScript      = $RawApp.installScript
        VerificationScript = $RawApp.verificationScript
        UpgradeScript      = $RawApp.upgradeScript
        InstallArgs        = $null
        PreventUpgrade     = $false
        Configuration      = $configuration
        IsDownloadApp      = $false
        Downloader         = $null
        DownloaderArgs     = $null
        DownloadedFilePath = $null
    }
}
