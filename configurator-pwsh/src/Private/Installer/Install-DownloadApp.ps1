function Install-DownloadApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$App
    )

    Write-ConfiguratorLog -Message "Downloading '$($App.AppId)'" -Level Info

    $downloaderName = $App.Downloader
    $downloaderArgs = $App.DownloaderArgs

    $filePath = $null
    switch ($downloaderName) {
        'GitHubAssetDownloader' {
            $filePath = Get-GitHubAsset -DownloaderArgs $downloaderArgs
        }
        'VisualStudioMarketplaceDownloader' {
            $filePath = Get-VisualStudioExtension -DownloaderArgs $downloaderArgs
        }
        default {
            throw "Cannot find downloader '$downloaderName'"
        }
    }

    $App.DownloadedFilePath = $filePath

    if ($App.InstallScript -match '\{\{DownloadedFilePath\}\}') {
        $App.InstallScript = $App.InstallScript -replace '\{\{DownloadedFilePath\}\}', $filePath
    }
    if ($App.UpgradeScript -match '\{\{DownloadedFilePath\}\}') {
        $App.UpgradeScript = $App.UpgradeScript -replace '\{\{DownloadedFilePath\}\}', $filePath
    }

    Write-ConfiguratorLog -Message "Downloaded '$($App.AppId)'" -Level Result

    Install-App -App $App
}
