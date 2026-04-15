function Install-App {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$App,

        [Parameter()]
        [switch]$ForceWindowsPowerShell
    )

    Write-ConfiguratorLog -Message "Installing '$($App.AppId)'" -Level Info

    $preInstallEntries = @(Get-DesktopEntries)

    $preInstallVerified = $false
    if ($null -ne $App.VerificationScript) {
        if ($ForceWindowsPowerShell) {
            $preInstallVerified = Invoke-WindowsPowerShellScript -Script $App.VerificationScript -ResultType ([bool])
        }
        else {
            $preInstallVerified = Invoke-PowerShellScript -Script $App.VerificationScript -ResultType ([bool])
        }
    }

    $actionScript = ''
    if (-not $preInstallVerified) {
        $actionScript = $App.InstallScript
    }
    elseif ($null -ne $App.UpgradeScript -and -not $App.PreventUpgrade) {
        $actionScript = $App.UpgradeScript
    }

    if (-not [string]::IsNullOrWhiteSpace($actionScript)) {
        if ($ForceWindowsPowerShell) {
            Invoke-WindowsPowerShellScript -Script $actionScript
        }
        else {
            Invoke-PowerShellScript -Script $actionScript
        }

        if ($null -ne $App.VerificationScript) {
            $postInstallVerified = $false
            if ($ForceWindowsPowerShell) {
                $postInstallVerified = Invoke-WindowsPowerShellScript -Script $App.VerificationScript -ResultType ([bool])
            }
            else {
                $postInstallVerified = Invoke-PowerShellScript -Script $App.VerificationScript -ResultType ([bool])
            }

            if (-not $postInstallVerified) {
                Write-ConfiguratorLog -Message "Failed to install '$($App.AppId)'" -Level Debug
            }
        }
    }

    $postInstallEntries = @(Get-DesktopEntries)
    Remove-DesktopShortcuts -BeforeEntries $preInstallEntries -AfterEntries $postInstallEntries

    Write-ConfiguratorLog -Message "Installed '$($App.AppId)'" -Level Result
}
