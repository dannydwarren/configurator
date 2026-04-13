function Set-ConfiguratorSetting {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [string]$Value
    )

    # TODO: Implement - load settings, find property by dotted path, convert value, set property, save
    throw "Not implemented"
}
