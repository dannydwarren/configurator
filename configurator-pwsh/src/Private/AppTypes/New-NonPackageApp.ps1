function New-NonPackageApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'nonPackageApp'
        Environments       = $environments
        InstallScript      = "./$($appId)_install.ps1"
        VerificationScript = $null
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
