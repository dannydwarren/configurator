function Invoke-PowerShellScript {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Script,

        [Parameter()]
        [switch]$RunAsAdmin,

        [Parameter()]
        [type]$ResultType
    )

    # TODO: Implement - wrap script with env setup, write to temp file, execute via pwsh.exe, handle output/errors/exit code
    throw "Not implemented"
}
