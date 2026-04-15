function New-ScriptFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Script
    )

    $tempDir = Join-Path $env:LOCALAPPDATA 'Configurator' 'temp'
    New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

    $timestamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss-fffff'
    $fileName = "$timestamp.ps1"
    $filePath = Join-Path $tempDir $fileName

    Set-Content -Path $filePath -Value $Script -Encoding UTF8

    $filePath
}
