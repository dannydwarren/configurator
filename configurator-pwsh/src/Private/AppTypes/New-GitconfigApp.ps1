function New-GitconfigApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    $escapedAppId = $appId.Replace('\', '\\')

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'gitconfig'
        Environments       = $environments
        InstallScript      = "git config --global --add include.path $appId"
        VerificationScript = "(git config --get-all --global include.path) -match `"$escapedAppId`""
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
