function New-ScoopBucketApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'scoopBucket'
        Environments       = $environments
        InstallScript      = "scoop bucket add $appId"
        VerificationScript = "(scoop bucket list | Select-String '$appId') -ne `$null"
        UpgradeScript      = $null
        InstallArgs        = $null
        PreventUpgrade     = $false
        Configuration      = $null
        IsDownloadApp      = $false
        Downloader         = $null
        DownloaderArgs     = $null
        DownloadedFilePath = $null
    }
}
