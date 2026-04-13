function New-WingetApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with Install/Verify/Upgrade scripts using winget commands
    # InstallArgs: if non-empty, prepend " --override "
    # Install: winget install --id {AppId} --accept-package-agreements -h -e{InstallArgs}
    # Verify:  (winget list --id {AppId} -e | Select-String {AppId}) -ne $null
    # Upgrade: winget upgrade --id {AppId} --accept-package-agreements -h -e{InstallArgs}
    throw "Not implemented"
}
