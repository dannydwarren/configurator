function Import-AppDefinition {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AppId,

        [Parameter(Mandatory)]
        [string]$ManifestDirectory
    )

    $appDir = Join-Path $ManifestDirectory 'apps' $AppId
    $appFilePath = Join-Path $appDir 'app.json'
    $json = Get-Content -Path $appFilePath -Raw
    $raw = $json | ConvertFrom-Json

    $raw
}
