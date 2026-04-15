function New-PowerShellModuleApp {
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
        AppType            = 'powerShellModule'
        Environments       = $environments
        InstallScript      = "Import-Module PowerShellGet -UseWindowsPowerShell`nInstall-Module -Name $appId$installArgs"
        VerificationScript = "(Get-Module -ListAvailable $appId) -ne `$null"
        UpgradeScript      = "Import-Module PowerShellGet -UseWindowsPowerShell`nUpdate-Module -Name $appId$installArgs"
        InstallArgs        = $installArgs
        PreventUpgrade     = $preventUpgrade
        Configuration      = $configuration
        IsDownloadApp      = $false
        Downloader         = $null
        DownloaderArgs     = $null
        DownloadedFilePath = $null
    }
}
