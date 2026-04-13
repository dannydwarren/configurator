function Set-WingetConfiguration {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Upgrade', 'AcceptSourceAgreements')]
        [string]$Action
    )

    $ErrorActionPreference = 'Stop'

    if ($Action -eq 'Upgrade') {
        Invoke-WindowsPowerShellScript -Script 'Add-AppxPackage https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle -ForceTargetApplicationShutdown'
        Invoke-WindowsPowerShellScript -Script 'Add-AppxPackage https://cdn.winget.microsoft.com/cache/source.msix'
    }
    else {
        Invoke-WindowsPowerShellScript -Script 'winget list winget --accept-source-agreements'
    }
}
