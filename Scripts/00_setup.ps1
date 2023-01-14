$scriptsPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$localPublishPath = "$($scriptsPath)\local-publish.ps1"
$isLocalPublishAddedToProfile = ((Test-Path $profile) -and (Select-String ($localPublishPath -replace "\\", "\\") $profile))

if (-not $isLocalPublishAddedToProfile) {
     Add-Content -Path $profile -Value "`n. $($localPublishPath)"
     Write-Host "Added supporting files."
} else {
    Write-Host "Supporting files already added."
}