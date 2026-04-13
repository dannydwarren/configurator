[CmdletBinding(DefaultParameterSetName = 'Help')]
param(
    [Parameter(ParameterSetName = 'Initialize', Mandatory)]
    [switch]$Initialize,

    [Parameter(ParameterSetName = 'ConfigureMachine', Mandatory)]
    [Alias('configure')]
    [switch]$ConfigureMachine,

    [Parameter(ParameterSetName = 'ConfigureMachine')]
    [Alias('e')]
    [string]$Environments,

    [Parameter(ParameterSetName = 'ConfigureMachine')]
    [Alias('app')]
    [string]$SingleAppId,

    [Parameter(ParameterSetName = 'SettingsList', Mandatory)]
    [switch]$SettingsList,

    [Parameter(ParameterSetName = 'SettingsSet', Mandatory)]
    [switch]$SettingsSet,

    [Parameter(ParameterSetName = 'SettingsSet', Mandatory, Position = 0)]
    [string]$SettingName,

    [Parameter(ParameterSetName = 'SettingsSet', Mandatory, Position = 1)]
    [string]$SettingValue,

    [Parameter(ParameterSetName = 'AddApp', Mandatory)]
    [Alias('add')]
    [switch]$AddApp,

    [Parameter(ParameterSetName = 'AddApp', Mandatory)]
    [string]$AppId,

    [Parameter(ParameterSetName = 'AddApp', Mandatory)]
    [string]$AppType,

    [Parameter(ParameterSetName = 'AddApp', Mandatory)]
    [string]$AppEnvironments,

    [Parameter(ParameterSetName = 'Backup', Mandatory)]
    [switch]$Backup,

    [Parameter(ParameterSetName = 'Help')]
    [switch]$Help
)

$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'Configurator.psd1') -Force

# TODO: Implement CLI routing to Public functions based on parameter set
# Check elevation, parse arguments, route to appropriate command
throw "Not implemented"
