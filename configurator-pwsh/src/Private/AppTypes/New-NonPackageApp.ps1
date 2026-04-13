function New-NonPackageApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with convention-based install script
    # Install: ./{AppId}_install.ps1
    # Verify:  $null
    # Upgrade: $null
    throw "Not implemented"
}
