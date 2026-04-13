function Initialize-System {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    Set-PowerShellPolicy -Edition Windows
    Set-WingetConfiguration -Action Upgrade
    Set-WingetConfiguration -Action AcceptSourceAgreements
    Install-PowerShellCore
    Set-PowerShellPolicy -Edition Core
    Install-Self
    Install-ScoopCli
    Install-Git
    Install-ManifestRepo
}
