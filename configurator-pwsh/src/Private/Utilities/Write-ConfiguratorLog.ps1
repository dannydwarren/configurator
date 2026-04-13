function Write-ConfiguratorLog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [Parameter()]
        [ValidateSet('Debug', 'Verbose', 'Info', 'Warn', 'Error', 'Progress', 'Result')]
        [string]$Level = 'Info'
    )

    # TODO: Implement - format as "[<ISO8601>] [<LEVEL>] <message>" with appropriate console color
    throw "Not implemented"
}
