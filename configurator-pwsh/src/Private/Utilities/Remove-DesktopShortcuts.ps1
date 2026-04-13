function Remove-DesktopShortcuts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string[]]$PreInstallEntries,

        [Parameter(Mandatory)]
        [string[]]$PostInstallEntries
    )

    # TODO: Implement - compute diff between pre and post entries, delete new files/directories
    throw "Not implemented"
}
