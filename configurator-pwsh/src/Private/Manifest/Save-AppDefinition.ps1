function Save-AppDefinition {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$AppId,

        [Parameter(Mandatory)]
        [string]$AppType,

        [Parameter(Mandatory)]
        [string]$Environments
    )

    $settings = Import-Settings
    $manifestDir = $settings.manifest.directory
    $manifestFileName = $settings.manifest.fileName
    $manifestFilePath = Join-Path $manifestDir $manifestFileName

    if (-not (Test-Path $manifestFilePath)) {
        Set-Content -Path $manifestFilePath -Value '{}' -Encoding UTF8
    }

    $manifestJson = Get-Content -Path $manifestFilePath -Raw | ConvertFrom-Json

    $appsList = @()
    if ($null -ne $manifestJson.apps) {
        $appsList = @($manifestJson.apps)
    }

    if ($appsList -contains $AppId) {
        return
    }

    $appDir = Join-Path $manifestDir 'apps' $AppId
    New-Item -Path $appDir -ItemType Directory -Force | Out-Null

    $appDefinition = [PSCustomObject]@{
        appType      = $AppType
        appId        = $AppId
        environments = $Environments
    }

    $appJson = $appDefinition | ConvertTo-Json -Depth 10
    $appFilePath = Join-Path $appDir 'app.json'
    Set-Content -Path $appFilePath -Value $appJson -Encoding UTF8

    $appsList += $AppId

    $manifestFile = [PSCustomObject]@{
        apps = $appsList
    }

    $manifestFileJson = $manifestFile | ConvertTo-Json -Depth 10
    Set-Content -Path $manifestFilePath -Value $manifestFileJson -Encoding UTF8
}
