function New-VisualStudioExtensionApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with VSIXInstaller commands, implements download app pattern
    # Downloader: VisualStudioMarketplaceDownloader (hardcoded)
    # Install script uses vswhere.exe to find VSIXInstaller.exe (FIX: $vsixInstaller not $vsi0xInstaller)
    # Verify: $null
    # Upgrade: same as Install
    throw "Not implemented"
}
