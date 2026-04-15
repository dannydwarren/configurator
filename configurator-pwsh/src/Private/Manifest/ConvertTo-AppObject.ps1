function ConvertTo-AppObject {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$RawApp,

        [Parameter(Mandatory)]
        [PSCustomObject]$Settings
    )

    switch ($RawApp.appType) {
        'winget' { return New-WingetApp -RawApp $RawApp }
        'scoop' { return New-ScoopApp -RawApp $RawApp }
        'scoopBucket' { return New-ScoopBucketApp -RawApp $RawApp }
        'powerShell' { return New-PowerShellApp -RawApp $RawApp -ManifestDirectory $Settings.manifest.directory }
        'powerShellModule' { return New-PowerShellModuleApp -RawApp $RawApp }
        'powerShellAppPackage' { return New-PowerShellAppPackageApp -RawApp $RawApp }
        'script' { return New-ScriptApp -RawApp $RawApp }
        'gitRepo' { return New-GitRepoApp -RawApp $RawApp -CloneRootDirectory $Settings.git.cloneDirectory }
        'gitconfig' { return New-GitconfigApp -RawApp $RawApp }
        'nonPackageApp' { return New-NonPackageApp -RawApp $RawApp }
        'visualStudioExtension' { return New-VisualStudioExtensionApp -RawApp $RawApp }
        default { return $null }
    }
}
