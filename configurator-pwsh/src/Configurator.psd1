@{
    RootModule        = 'Configurator.psm1'
    ModuleVersion     = '0.0.1'
    GUID              = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'
    Author            = 'Danny Warren'
    CompanyName       = 'dannydwarren'
    Description       = 'Windows machine configuration tool'
    PowerShellVersion = '7.0'

    FunctionsToExport = @(
        'Invoke-Initialize'
        'Invoke-ConfigureMachine'
        'Get-ConfiguratorSettings'
        'Set-ConfiguratorSetting'
        'Add-ConfiguratorApp'
        'Invoke-Backup'
    )

    CmdletsToExport   = @()
    VariablesToExport  = @()
    AliasesToExport    = @()

    PrivateData = @{
        PSData = @{
            Tags       = @('Windows', 'Configuration', 'Machine', 'Setup')
            ProjectUri = 'https://github.com/dannydwarren/configurator'
        }
    }
}
