function Install-ManifestRepo {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    $settings = Import-Settings

    $repoUri = $settings.manifest.repo
    if ($null -eq $repoUri -or $repoUri -eq '') {
        throw "Missing setting: Manifest.Repo"
    }

    $cloneDir = $settings.git.cloneDirectory

    $app = New-GitRepoApp -RawApp ([PSCustomObject]@{
        appId        = 'git.manifest-repo'
        environments = ''
        installArgs  = $repoUri.ToString()
    }) -CloneRootDirectory $cloneDir.ToString()

    Install-App -App $app
}
