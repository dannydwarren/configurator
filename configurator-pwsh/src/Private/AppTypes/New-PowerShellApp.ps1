function New-PowerShellApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp,

        [Parameter(Mandatory)]
        [string]$ManifestDirectory
    )

    # TODO: Implement - load scripts from files in apps/<appId>/ directory
    # Install:      . "<manifest>/apps/<appId>/install.ps1"   (required - return $null if missing)
    # Upgrade:      . "<manifest>/apps/<appId>/upgrade.ps1"   (optional)
    # Verification: . "<manifest>/apps/<appId>/verification.ps1" (optional)
    throw "Not implemented"
}
