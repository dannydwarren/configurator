function New-GitconfigApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with git config include.path commands
    # Install: git config --global --add include.path {AppId}
    # Verify:  (git config --get-all --global include.path) -match "{AppId}"  (backslashes escaped/doubled)
    # Upgrade: $null
    throw "Not implemented"
}
