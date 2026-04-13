function Import-AppDefinition {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AppId,

        [Parameter(Mandatory)]
        [string]$ManifestDirectory
    )

    # TODO: Implement - read apps/<appId>/app.json, return raw PSCustomObject with AppData
    throw "Not implemented"
}
