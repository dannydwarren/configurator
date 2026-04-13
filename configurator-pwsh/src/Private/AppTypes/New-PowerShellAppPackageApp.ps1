function New-PowerShellAppPackageApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments

    $preventUpgrade = $false
    if ($null -ne $RawApp.preventUpgrade) {
        $preventUpgrade = [bool]$RawApp.preventUpgrade
    }

    $installScript = "Import-Module appx -UseWindowsPowerShell`nAdd-AppPackage `$DownloadedFilePath"
    $verificationScript = "Import-Module appx -UseWindowsPowerShell`n(Get-AppPackage -Name $appId) -ne `$null"

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'powerShellAppPackage'
        Environments       = $environments
        InstallScript      = $installScript
        VerificationScript = $verificationScript
        UpgradeScript      = $installScript
        InstallArgs        = $null
        PreventUpgrade     = $preventUpgrade
        Configuration      = $null
        IsDownloadApp      = $true
        Downloader         = $RawApp.downloader
        DownloaderArgs     = $RawApp.downloaderArgs
        DownloadedFilePath = $null
    }
}
