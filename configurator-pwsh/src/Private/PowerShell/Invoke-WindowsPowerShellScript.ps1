function Invoke-WindowsPowerShellScript {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Script,

        [Parameter()]
        [switch]$RunAsAdmin,

        [Parameter()]
        [type]$ResultType
    )

    # TODO: Implement - same as Invoke-PowerShellScript but uses powershell.exe and WindowsPowerShell profile path
    throw "Not implemented"
}
