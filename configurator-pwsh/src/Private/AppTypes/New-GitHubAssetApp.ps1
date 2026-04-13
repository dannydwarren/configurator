function New-GitHubAssetApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = if ($null -ne $RawApp.environments) { $RawApp.environments } else { '' }

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'gitHubAsset'
        Environments       = $environments
        InstallScript      = ''
        VerificationScript = $null
        UpgradeScript      = $null
        InstallArgs        = $null
        PreventUpgrade     = $true
        Configuration      = $null
        IsDownloadApp      = $true
        Downloader         = 'GitHubAssetDownloader'
        DownloaderArgs     = $RawApp.downloaderArgs
        DownloadedFilePath = $null
    }
}
