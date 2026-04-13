BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Configurator Module' {
    It 'imports without errors' {
        $module = Get-Module -Name 'Configurator'
        $module | Should -Not -BeNullOrEmpty
    }

    It 'exports expected public functions' {
        $expectedFunctions = @(
            'Invoke-Initialize'
            'Invoke-ConfigureMachine'
            'Get-ConfiguratorSettings'
            'Set-ConfiguratorSetting'
            'Add-ConfiguratorApp'
            'Invoke-Backup'
            'Start-Configurator'
        )

        $module = Get-Module -Name 'Configurator'
        foreach ($fn in $expectedFunctions) {
            $module.ExportedFunctions.Keys | Should -Contain $fn
        }
    }
}
