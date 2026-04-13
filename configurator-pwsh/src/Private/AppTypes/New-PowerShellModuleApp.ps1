function New-PowerShellModuleApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with Install-Module / Update-Module scripts
    # InstallArgs: if non-empty, prepend " "
    # Install: Import-Module PowerShellGet -UseWindowsPowerShell\nInstall-Module -Name {AppId}{InstallArgs}
    # Verify:  (Get-Module -ListAvailable {AppId}) -ne $null
    # Upgrade: Import-Module PowerShellGet -UseWindowsPowerShell\nUpdate-Module -Name {AppId}{InstallArgs}
    throw "Not implemented"
}
