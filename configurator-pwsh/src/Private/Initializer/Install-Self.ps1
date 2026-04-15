function Install-Self {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    $installDir = 'c:\Configurator'

    if (Test-Path $installDir) {
        return
    }

    $app = New-GitHubAssetApp -RawApp ([PSCustomObject]@{
        appId          = 'Configurator'
        downloaderArgs = [PSCustomObject]@{
            User      = 'dannydwarren'
            Repo      = 'configurator'
            Extension = '.exe'
        }
    })

    Install-DownloadApp -App $app

    New-Item -Path $installDir -ItemType Directory -Force | Out-Null
    Move-Item -Path $app.DownloadedFilePath -Destination (Join-Path $installDir 'Configurator.exe') -Force

    Add-ToMachinePath -Directory $installDir
}

function Add-ToMachinePath {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Directory
    )

    $machinePath = [System.Environment]::GetEnvironmentVariable('Path', 'Machine')
    if ($machinePath -notlike "*$Directory*") {
        [System.Environment]::SetEnvironmentVariable('Path', "$machinePath;$Directory", 'Machine')
    }
}
