function Invoke-AppConfigurator {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$App,

        [Parameter()]
        [switch]$Configure,

        [Parameter()]
        [switch]$Backup
    )

    # TODO: Implement - Configure: apply registry settings from app.Configuration
    # TODO: Implement - Backup: verify app installed, run backup.ps1 if exists
    throw "Not implemented"
}
