function New-GitHubAssetApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - internal-only download app type (not exposed via manifest AppType)
    # InstallScript: empty string (no-op)
    # VerificationScript: $null
    # UpgradeScript: $null
    # PreventUpgrade: $true
    # Downloader: GitHubAssetDownloader
    throw "Not implemented"
}
