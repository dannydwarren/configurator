function New-ScriptApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with inline scripts from app.json fields
    # installScript, verificationScript, upgradeScript taken directly from JSON
    throw "Not implemented"
}
