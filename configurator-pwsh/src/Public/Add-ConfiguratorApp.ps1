function Add-ConfiguratorApp {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AppId,

        [Parameter(Mandatory)]
        [ValidateSet(
            'winget', 'scoop', 'scoopBucket', 'powerShell', 'powerShellModule',
            'powerShellAppPackage', 'script', 'gitRepo', 'gitconfig',
            'nonPackageApp', 'visualStudioExtension'
        )]
        [string]$AppType,

        [Parameter(Mandatory)]
        [string[]]$Environments
    )

    $joinedEnvironments = $Environments -join '|'

    Save-AppDefinition -AppId $AppId -AppType $AppType -Environments $joinedEnvironments
}
