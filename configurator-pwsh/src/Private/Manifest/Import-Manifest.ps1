function Import-Manifest {
    [CmdletBinding()]
    param(
        [Parameter()]
        [string[]]$Environments = @(),

        [Parameter()]
        [string]$SingleAppId
    )

    $settings = Import-Settings
    $manifestDir = $settings.manifest.directory
    $manifestFileName = $settings.manifest.fileName
    $manifestFilePath = Join-Path $manifestDir $manifestFileName

    if (-not (Test-Path $manifestFilePath)) {
        Set-Content -Path $manifestFilePath -Value '{}' -Encoding UTF8
    }

    $manifestJson = Get-Content -Path $manifestFilePath -Raw | ConvertFrom-Json

    $appIds = @()
    if ($null -ne $manifestJson.apps) {
        $appIds = @($manifestJson.apps)
    }

    $rawApps = foreach ($id in $appIds) {
        Import-AppDefinition -AppId $id -ManifestDirectory $manifestDir
    }

    if (-not [string]::IsNullOrWhiteSpace($SingleAppId)) {
        $apps = @()
        foreach ($raw in $rawApps) {
            $app = ConvertTo-AppObject -RawApp $raw -Settings $settings
            if ($null -ne $app) {
                $apps += $app
            }
        }
        $match = $apps | Where-Object { $_.AppId -eq $SingleAppId } | Select-Object -First 1
        if ($null -ne $match) {
            $apps = @($match)
        }
        else {
            $apps = @()
        }

        return @{
            AppIds = $appIds
            Apps   = $apps
        }
    }

    $filtered = foreach ($raw in $rawApps) {
        $include = $false

        if ($Environments.Count -eq 0) {
            $include = $true
        }
        else {
            $envLowered = $raw.environments.ToLower()
            foreach ($env in $Environments) {
                if ($envLowered.Contains($env.ToLower())) {
                    $include = $true
                    break
                }
            }
        }

        if ($include) {
            $raw
        }
    }

    $apps = @()
    foreach ($raw in $filtered) {
        $app = ConvertTo-AppObject -RawApp $raw -Settings $settings
        if ($null -ne $app) {
            $apps += $app
        }
    }

    @{
        AppIds = $appIds
        Apps   = $apps
    }
}
