function New-ScoopApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    $installArgs = ''
    if (-not [string]::IsNullOrWhiteSpace($RawApp.installArgs)) {
        $installArgs = " $($RawApp.installArgs)"
    }

    $preventUpgrade = $false
    if ($null -ne $RawApp.preventUpgrade) {
        $preventUpgrade = [bool]$RawApp.preventUpgrade
    }

    $configuration = $null
    if ($null -ne $RawApp.configuration) {
        $configuration = $RawApp.configuration
    }

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'scoop'
        Environments       = $environments
        InstallScript      = "scoop install $appId$installArgs"
        VerificationScript = "(scoop export | Select-String $appId) -ne `$null"
        UpgradeScript      = "scoop update $appId"
        InstallArgs        = $installArgs
        PreventUpgrade     = $preventUpgrade
        Configuration      = $configuration
        IsDownloadApp      = $false
        Downloader         = $null
        DownloaderArgs     = $null
        DownloadedFilePath = $null
    }
}
