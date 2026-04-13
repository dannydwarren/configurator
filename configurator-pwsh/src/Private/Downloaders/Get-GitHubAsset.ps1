function Get-GitHubAsset {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$DownloaderArgs
    )

    $user = $DownloaderArgs.User
    $repo = $DownloaderArgs.Repo
    $extension = $DownloaderArgs.Extension

    $getAssetInfoScript = @"
`$asset = (Invoke-WebRequest https://api.github.com/repos/$user/$repo/releases/latest | ConvertFrom-Json).assets | Where-Object { `$_.name -like '*$extension' }
`$downloadUrl = `$asset | Select-Object -ExpandProperty browser_download_url
`$fileName = `$asset | Select-Object -ExpandProperty name
Write-Output "{ ``"FileName``": ``"`$fileName``", ``"Url``": ``"`$downloadUrl``" }"
"@

    $result = Invoke-PowerShellScript -Script $getAssetInfoScript -ResultType ([string])
    $assetInfo = $result | ConvertFrom-Json

    $settings = Import-Settings
    Get-Download -Url $assetInfo.Url -FileName $assetInfo.FileName -DownloadsDirectory $settings.downloadsDirectory
}
