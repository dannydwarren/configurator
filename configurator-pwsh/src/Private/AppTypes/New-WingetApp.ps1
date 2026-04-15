function New-WingetApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    $installArgs = ''
    if (-not [string]::IsNullOrWhiteSpace($RawApp.installArgs)) {
        $installArgs = " --override $($RawApp.installArgs)"
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
        AppType            = 'winget'
        Environments       = $environments
        InstallScript      = "winget install --id $appId --accept-package-agreements -h -e$installArgs"
        VerificationScript = "(winget list --id $appId -e | Select-String $appId) -ne `$null"
        UpgradeScript      = "winget upgrade --id $appId --accept-package-agreements -h -e$installArgs"
        InstallArgs        = $installArgs
        PreventUpgrade     = $preventUpgrade
        Configuration      = $configuration
        IsDownloadApp      = $false
        Downloader         = $null
        DownloaderArgs     = $null
        DownloadedFilePath = $null
    }
}
