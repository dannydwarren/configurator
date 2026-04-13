function Export-Settings {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$Settings
    )

    $ErrorActionPreference = 'Stop'

    $settingsPath = Get-SettingsPath
    $json = $Settings | ConvertTo-Json -Depth 10
    Set-Content -Path $settingsPath -Value $json -Encoding UTF8

    New-Item -Path $Settings.downloadsDirectory -ItemType Directory -Force | Out-Null
}
