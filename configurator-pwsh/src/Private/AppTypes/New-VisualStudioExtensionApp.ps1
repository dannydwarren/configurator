function New-VisualStudioExtensionApp {
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

    $installScript = @'
$vsixInstaller = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -property productPath | Split-Path | % { "$_\VSIXInstaller.exe" }
$installArgs = "/quiet", "/admin", "$DownloadedFilePath"
Start-Process $vsixInstaller $installArgs -Wait
'@

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'visualStudioExtension'
        Environments       = $environments
        InstallScript      = $installScript
        VerificationScript = $null
        UpgradeScript      = $installScript
        InstallArgs        = $null
        PreventUpgrade     = $preventUpgrade
        Configuration      = $null
        IsDownloadApp      = $true
        Downloader         = 'VisualStudioMarketplaceDownloader'
        DownloaderArgs     = $RawApp.downloaderArgs
        DownloadedFilePath = $null
    }
}
