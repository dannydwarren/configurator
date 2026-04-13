#Requires -RunAsAdministrator
param(
    [string]$InstallPath = 'C:\Configurator',
    [string]$GitHubUser = 'dannydwarren',
    [string]$GitHubRepo = 'configurator'
)

$ErrorActionPreference = 'Stop'

Write-Host "Fetching latest PowerShell release from GitHub..."
$releaseInfo = Invoke-RestMethod -Uri "https://api.github.com/repos/$GitHubUser/$GitHubRepo/releases" -Headers @{ 'User-Agent' = 'Configurator-Installer' }

$pwshRelease = $releaseInfo | Where-Object { $_.tag_name -like 'pwsh-v*' } | Select-Object -First 1

if (-not $pwshRelease) {
    throw "No PowerShell release found in $GitHubUser/$GitHubRepo"
}

$zipAsset = $pwshRelease.assets | Where-Object { $_.name -like 'Configurator-v*.zip' } | Select-Object -First 1

if (-not $zipAsset) {
    throw "No zip asset found in release $($pwshRelease.tag_name)"
}

$downloadUrl = $zipAsset.browser_download_url
$tempZip = Join-Path $env:TEMP "Configurator-latest.zip"

Write-Host "Downloading $($zipAsset.name) from $($pwshRelease.tag_name)..."
Invoke-WebRequest -Uri $downloadUrl -OutFile $tempZip -UseBasicParsing

if (Test-Path $InstallPath) {
    Write-Host "Removing existing installation at $InstallPath..."
    Remove-Item -Path $InstallPath -Recurse -Force
}

Write-Host "Extracting to $InstallPath..."
$tempExtract = Join-Path $env:TEMP "Configurator-extract"
if (Test-Path $tempExtract) {
    Remove-Item -Path $tempExtract -Recurse -Force
}
Expand-Archive -Path $tempZip -DestinationPath $tempExtract -Force

$extractedDir = Join-Path $tempExtract 'Configurator'
if (Test-Path $extractedDir) {
    Move-Item -Path $extractedDir -Destination $InstallPath -Force
} else {
    New-Item -Path $InstallPath -ItemType Directory -Force | Out-Null
    Copy-Item -Path "$tempExtract\*" -Destination $InstallPath -Recurse -Force
}

$machinePath = [System.Environment]::GetEnvironmentVariable('Path', 'Machine')
if ($machinePath -notlike "*$InstallPath*") {
    Write-Host "Adding $InstallPath to machine PATH..."
    [System.Environment]::SetEnvironmentVariable('Path', "$machinePath;$InstallPath", 'Machine')
}

$srcPath = Join-Path $InstallPath 'src'
if (-not (Test-Path $srcPath)) {
    $srcPath = $InstallPath
}

$currentModulePath = [System.Environment]::GetEnvironmentVariable('PSModulePath', 'Machine')
if ($currentModulePath -notlike "*$srcPath*") {
    Write-Host "Adding $srcPath to machine PSModulePath..."
    [System.Environment]::SetEnvironmentVariable('PSModulePath', "$currentModulePath;$srcPath", 'Machine')
}

Remove-Item -Path $tempZip -Force -ErrorAction SilentlyContinue
Remove-Item -Path $tempExtract -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Configurator installed to $InstallPath"
Write-Host "Restart your shell, then run: Import-Module Configurator"
