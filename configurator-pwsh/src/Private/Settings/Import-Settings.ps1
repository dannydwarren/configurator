function Import-Settings {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    $settingsPath = Get-SettingsPath

    if (-not (Test-Path $settingsPath)) {
        $settingsDir = Split-Path $settingsPath -Parent
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null

        $defaults = [PSCustomObject]@{
            downloadsDirectory = 'C:/tmp/configurator-downloads'
            manifest           = [PSCustomObject]@{
                repo      = $null
                fileName  = 'manifest.json'
                directory = $null
            }
            git                = [PSCustomObject]@{
                cloneDirectory = 'C:\src\'
            }
        }

        $json = $defaults | ConvertTo-Json -Depth 10
        Set-Content -Path $settingsPath -Value $json -Encoding UTF8

        New-Item -Path $defaults.downloadsDirectory -ItemType Directory -Force | Out-Null

        return $defaults
    }

    $settings = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json

    New-Item -Path $settings.downloadsDirectory -ItemType Directory -Force | Out-Null

    $settings
}
