function New-ScoopApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with Install/Verify/Upgrade scripts using scoop commands
    # InstallArgs: if non-empty, prepend " "
    # Install: scoop install {AppId}{InstallArgs}
    # Verify:  (scoop export | Select-String {AppId}) -ne $null
    # Upgrade: scoop update {AppId}
    throw "Not implemented"
}
