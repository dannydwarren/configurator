BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Settings Integration - Load Before Save' {
    It 'returns sensible defaults on first load' {
        InModuleScope Configurator {
            $settingsDir = Join-Path $TestDrive 'settings-first-load'
            $settingsPath = Join-Path $settingsDir 'settings.json'

            Mock Get-SettingsPath { return $settingsPath }

            $settings = Import-Settings

            $settings | Should -Not -BeNullOrEmpty
            $settings.downloadsDirectory | Should -Be 'C:/tmp/configurator-downloads'
            $settings.manifest.fileName | Should -Be 'manifest.json'
            $settings.manifest.repo | Should -BeNullOrEmpty
            $settings.manifest.directory | Should -BeNullOrEmpty
            $settings.git.cloneDirectory | Should -Be 'C:\src\'

            Test-Path $settingsPath | Should -BeTrue
        }
    }
}

Describe 'Settings Integration - Load After Save' {
    It 'returns saved settings after export' {
        InModuleScope Configurator {
            $settingsDir = Join-Path $TestDrive 'settings-save-load'
            New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
            $settingsPath = Join-Path $settingsDir 'settings.json'

            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            $settings = Import-Settings

            $newFileName = 'custom-manifest.json'
            $settings.manifest | Add-Member -MemberType NoteProperty -Name 'fileName' -Value $newFileName -Force

            Export-Settings -Settings $settings

            $reloaded = Import-Settings

            $reloaded.manifest.fileName | Should -Be $newFileName
        }
    }
}
