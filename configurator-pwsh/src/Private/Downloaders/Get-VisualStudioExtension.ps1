function Get-VisualStudioExtension {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ArgsJson
    )

    # TODO: Implement - parse Publisher/ExtensionName from JSON, fetch marketplace page, regex match download URL, download as .vsix
    throw "Not implemented"
}
