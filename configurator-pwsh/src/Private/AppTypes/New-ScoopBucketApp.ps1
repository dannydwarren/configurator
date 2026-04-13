function New-ScoopBucketApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp
    )

    # TODO: Implement - build PSCustomObject with scoop bucket commands
    # Install: scoop bucket add {AppId}
    # Verify:  (scoop bucket list | Select-String {AppId}) -ne $null  (FIX: properly interpolate AppId)
    # Upgrade: $null
    throw "Not implemented"
}
