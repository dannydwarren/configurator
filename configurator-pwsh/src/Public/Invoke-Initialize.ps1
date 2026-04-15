function Invoke-Initialize {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    Initialize-System

    $settings = Import-Settings

    if ($null -eq $settings.manifest.repo -or $settings.manifest.repo -eq '') {
        throw "The 'manifest.repo' setting must be set before invoking initialize."
    }

    $repoUrl = $settings.manifest.repo.ToString()
    $repoName = $repoUrl.Split('/')[-1].Replace('.git', '')
    $cloneDir = $settings.git.cloneDirectory.ToString()
    $manifestDirectory = Join-Path $cloneDir $repoName

    if ($settings.manifest.directory -ne $manifestDirectory) {
        $settings.manifest.directory = $manifestDirectory
        Export-Settings -Settings $settings
    }

    if (-not (Test-Path $manifestDirectory)) {
        Invoke-PowerShellScript -Script @"
Push-Location $cloneDir
git clone $repoUrl
Pop-Location
"@
    }

    $manifestFilePath = Join-Path $manifestDirectory $settings.manifest.fileName

    if (-not (Test-Path $manifestFilePath)) {
        Set-Content -Path $manifestFilePath -Value '{ }' -Encoding UTF8

        Invoke-PowerShellScript -Script @"
Push-Location $manifestDirectory
git add .
git commit -m '[Configurator] Create manifest file'
git push
Pop-Location
"@
    }
}
