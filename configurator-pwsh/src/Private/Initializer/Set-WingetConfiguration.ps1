function Set-WingetConfiguration {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Upgrade', 'AcceptSourceAgreements')]
        [string]$Action
    )

    $ErrorActionPreference = 'Stop'

    if ($Action -eq 'Upgrade') {
        try {
            Invoke-WindowsPowerShellScript -Script 'Add-AppxPackage https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle -ForceTargetApplicationShutdown'
        }
        catch {
            Write-ConfiguratorLog -Message "Winget upgrade skipped: $_" -Level Debug
        }
        try {
            Invoke-WindowsPowerShellScript -Script 'Add-AppxPackage https://cdn.winget.microsoft.com/cache/source.msix'
        }
        catch {
            Write-ConfiguratorLog -Message "Winget source update skipped: $_" -Level Debug
        }
    }
    else {
        Invoke-WindowsPowerShellScript -Script 'winget list winget --accept-source-agreements'
    }
}
