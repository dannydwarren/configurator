[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Command,

    [Parameter(ValueFromRemainingArguments)]
    [string[]]$RemainingArgs
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'Configurator.psd1') -Force

exit (Start-Configurator -Command $Command -RemainingArgs $RemainingArgs)
