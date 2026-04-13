function Get-GitHubAsset {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$ArgsJson
    )

    # TODO: Implement - parse User/Repo/Extension from JSON, query GitHub API for latest release asset, download via Get-Download
    throw "Not implemented"
}
