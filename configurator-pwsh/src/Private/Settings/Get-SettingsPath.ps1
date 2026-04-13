function Get-SettingsPath {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    Join-Path $env:LOCALAPPDATA 'Configurator' 'settings.json'
}
