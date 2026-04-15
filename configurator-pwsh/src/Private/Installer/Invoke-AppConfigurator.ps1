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

    if ($Configure) {
        if ($null -eq $App.Configuration) {
            return
        }

        foreach ($setting in $App.Configuration.registrySettings) {
            $valueData = $setting.valueData
            if ($valueData -is [string]) {
                $valueData = Resolve-Token -Value $valueData
            }
            Set-RegistryValue -KeyName $setting.keyName -ValueName $setting.valueName -ValueData $valueData
        }
    }

    if ($Backup) {
        $isInstalled = $false
        if (-not [string]::IsNullOrEmpty($App.VerificationScript)) {
            $isInstalled = Invoke-PowerShellScript -Script $App.VerificationScript -ResultType ([bool])
        }

        if (-not $isInstalled) {
            return
        }

        $settings = Import-Settings
        $backupFilePath = Join-Path $settings.manifest.directory "apps" $App.AppId "backup.ps1"

        if (Test-Path $backupFilePath) {
            Write-ConfiguratorLog -Message "Backing up $($App.AppId)..." -Level Info
            Invoke-PowerShellScript -Script $backupFilePath
            Write-ConfiguratorLog -Message "Backed up $($App.AppId)!" -Level Result
        }
    }
}
