function New-PowerShellAppPackageApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with Add-AppPackage scripts, implements download app pattern
    # Install: Import-Module appx -UseWindowsPowerShell\nAdd-AppPackage {DownloadedFilePath}
    # Verify:  Import-Module appx -UseWindowsPowerShell\n(Get-AppPackage -Name {AppId}) -ne $null
    # Upgrade: same as Install
    # Downloader/DownloaderArgs from app.json
    throw "Not implemented"
}
