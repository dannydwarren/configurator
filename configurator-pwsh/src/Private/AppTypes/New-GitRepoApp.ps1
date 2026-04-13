function New-GitRepoApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp,

        [Parameter(Mandatory)]
        [string]$CloneRootDirectory
    )

    # TODO: Implement - build PSCustomObject with git clone/pull scripts
    # Return $null if InstallArgs is null/whitespace
    # RepoName derived from InstallArgs: strip .git, split on / or \, take last segment
    # Install: mkdir {CloneRootDirectory} -Force;pushd {CloneRootDirectory};git clone {InstallArgs};popd
    # Verify:  Test-Path {CloneRootDirectory}{RepoName}
    # Upgrade: pushd {CloneRootDirectory}{RepoName};git pull;popd
    throw "Not implemented"
}
