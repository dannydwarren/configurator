function Add-ConfiguratorApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AppId,

        [Parameter(Mandatory)]
        [string]$AppType,

        [Parameter(Mandatory)]
        [string[]]$Environments
    )

    # TODO: Implement - create installable, call Save-AppDefinition (idempotent add to manifest + write app.json)
    throw "Not implemented"
}
