function New-GitRepoApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp,

        [Parameter(Mandatory)]
        [string]$CloneRootDirectory
    )

    $appId = $RawApp.appId
    $environments = $RawApp.environments
    $installArgs = $RawApp.installArgs

    if ([string]::IsNullOrWhiteSpace($installArgs)) {
        return $null
    }

    $preventUpgrade = $false
    if ($null -ne $RawApp.preventUpgrade) {
        $preventUpgrade = [bool]$RawApp.preventUpgrade
    }

    $endsWithSlash = $CloneRootDirectory.EndsWith('\') -or $CloneRootDirectory.EndsWith('/')
    if (-not $endsWithSlash) {
        $CloneRootDirectory += '\'
    }

    $repoName = $installArgs.Replace('.git', '').Split('\', '/') | Select-Object -Last 1

    [PSCustomObject]@{
        AppId              = $appId
        AppType            = 'gitRepo'
        Environments       = $environments
        InstallScript      = "mkdir $CloneRootDirectory -Force;pushd $CloneRootDirectory;git clone $installArgs;popd"
        VerificationScript = "Test-Path $CloneRootDirectory$repoName"
        UpgradeScript      = "pushd $CloneRootDirectory$repoName;git pull;popd"
        InstallArgs        = $installArgs
        PreventUpgrade     = $preventUpgrade
        Configuration      = $null
        IsDownloadApp      = $false
        Downloader         = $null
        DownloaderArgs     = $null
        DownloadedFilePath = $null
    }
}
