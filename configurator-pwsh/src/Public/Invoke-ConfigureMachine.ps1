function Invoke-ConfigureMachine {
    [CmdletBinding()]
    param(
        [Parameter()]
        [string[]]$Environments = @(),

        [Parameter()]
        [string]$SingleAppId
    )

    # TODO: Implement - load manifest (filtered by environments or single app), install/upgrade each app, configure each app
    throw "Not implemented"
}
