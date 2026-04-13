function Install-App {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$App,

        [Parameter()]
        [switch]$ForceWindowsPowerShell
    )

    # TODO: Implement verify -> install/upgrade -> re-verify -> configure -> delete shortcuts flow
    throw "Not implemented"
}
