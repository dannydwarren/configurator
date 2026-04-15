function Install-PowerShellCore {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    $app = New-WingetApp -RawApp ([PSCustomObject]@{
        appId        = 'Microsoft.PowerShell'
        environments = ''
    })

    Install-App -App $app -ForceWindowsPowerShell
}
