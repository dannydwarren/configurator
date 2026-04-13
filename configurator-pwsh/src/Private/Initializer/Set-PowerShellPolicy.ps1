function Set-PowerShellPolicy {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Core', 'Windows')]
        [string]$Edition
    )

    # TODO: Implement - Set-ExecutionPolicy RemoteSigned -Force (admin), report policy and version
    throw "Not implemented"
}
