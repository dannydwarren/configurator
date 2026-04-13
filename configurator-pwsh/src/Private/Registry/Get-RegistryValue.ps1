function Get-RegistryValue {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$KeyName,

        [Parameter(Mandatory)]
        [string]$ValueName
    )

    # TODO: Implement - read registry value, convert int to uint / long to ulong, return as string
    throw "Not implemented"
}
