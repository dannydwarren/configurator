function Set-WingetConfiguration {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Upgrade', 'AcceptSourceAgreements')]
        [string]$Action
    )

    # TODO: Implement - Upgrade: Add-AppxPackage for winget msixbundle and source.msix via Windows PowerShell
    # TODO: Implement - AcceptSourceAgreements: winget list winget --accept-source-agreements via Windows PowerShell
    throw "Not implemented"
}
