$ErrorActionPreference = 'Stop'

$privatePath = Join-Path $PSScriptRoot 'Private'
$publicPath = Join-Path $PSScriptRoot 'Public'

Get-ChildItem -Path $privatePath -Recurse -Filter '*.ps1' | ForEach-Object {
    . $_.FullName
}

Get-ChildItem -Path $publicPath -Recurse -Filter '*.ps1' | ForEach-Object {
    . $_.FullName
}

$publicFunctions = Get-ChildItem -Path $publicPath -Recurse -Filter '*.ps1' |
    ForEach-Object { $_.BaseName }

Export-ModuleMember -Function $publicFunctions
