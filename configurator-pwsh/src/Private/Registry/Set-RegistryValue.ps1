function Set-RegistryValue {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$KeyName,

        [Parameter(Mandatory)]
        [string]$ValueName,

        [Parameter(Mandatory)]
        $ValueData
    )

    # TODO: Implement - determine RegistryValueKind from value type (string -> String, uint -> DWord), write via Set-ItemProperty
    throw "Not implemented"
}
