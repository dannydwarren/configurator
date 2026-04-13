function ConvertTo-AppObject {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp,

        [Parameter(Mandatory)]
        [PSCustomObject]$Settings
    )

    # TODO: Implement - switch on appType, route to New-*App function, return standardized PSCustomObject (or $null for unknown)
    throw "Not implemented"
}
