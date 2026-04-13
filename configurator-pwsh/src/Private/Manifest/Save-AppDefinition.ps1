function Save-AppDefinition {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$Installable
    )

    # TODO: Implement - check if app already in manifest (idempotent), create app dir, write app.json, add to manifest, write manifest
    throw "Not implemented"
}
