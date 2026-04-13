function Install-Git {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    $app = New-WingetApp -RawApp ([PSCustomObject]@{
        appId        = 'Git.Git'
        environments = ''
    })

    Install-App -App $app
}
