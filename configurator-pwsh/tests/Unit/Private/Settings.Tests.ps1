BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Get-SettingsPath' {
    It 'returns the correct settings file path' {
        $result = InModuleScope Configurator { Get-SettingsPath }
        $expected = Join-Path $env:LOCALAPPDATA 'Configurator' 'settings.json'
        $result | Should -Be $expected
    }
}

Describe 'Import-Settings' {
    BeforeAll {
        $script:testRoot = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-settings-tests-$([guid]::NewGuid().ToString('N'))"
        New-Item -Path $script:testRoot -ItemType Directory -Force | Out-Null
    }

    AfterAll {
        if (Test-Path $script:testRoot) {
            Remove-Item -Path $script:testRoot -Recurse -Force
        }
    }

    It 'creates directory and default settings file on first load' {
        $settingsDir = Join-Path $script:testRoot 'first-load'
        $settingsPath = Join-Path $settingsDir 'settings.json'
        $downloadsDir = Join-Path $script:testRoot 'downloads'

        InModuleScope Configurator -Parameters @{ settingsPath = $settingsPath; downloadsDir = $downloadsDir } {
            param($settingsPath, $downloadsDir)
            Mock Get-SettingsPath { return $settingsPath }

            $result = Import-Settings

            $result.downloadsDirectory | Should -Be 'C:/tmp/configurator-downloads'
            $result.manifest.fileName | Should -Be 'manifest.json'
            $result.manifest.repo | Should -BeNullOrEmpty
            $result.manifest.directory | Should -BeNullOrEmpty
            $result.git.cloneDirectory | Should -Be 'C:\src\'

            Test-Path $settingsPath | Should -BeTrue
        }
    }

    It 'loads existing settings from disk' {
        $settingsDir = Join-Path $script:testRoot 'existing'
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
        $settingsPath = Join-Path $settingsDir 'settings.json'

        $existingSettings = @{
            downloadsDirectory = 'D:/my-downloads'
            manifest = @{
                repo = 'https://github.com/user/repo.git'
                fileName = 'custom-manifest.json'
                directory = 'D:\repos\my-config'
            }
            git = @{
                cloneDirectory = 'D:\repos\'
            }
        } | ConvertTo-Json -Depth 10
        Set-Content -Path $settingsPath -Value $existingSettings -Encoding UTF8

        InModuleScope Configurator -Parameters @{ settingsPath = $settingsPath } {
            param($settingsPath)
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            $result = Import-Settings

            $result.downloadsDirectory | Should -Be 'D:/my-downloads'
            $result.manifest.repo | Should -Be 'https://github.com/user/repo.git'
            $result.manifest.fileName | Should -Be 'custom-manifest.json'
            $result.manifest.directory | Should -Be 'D:\repos\my-config'
            $result.git.cloneDirectory | Should -Be 'D:\repos\'
        }
    }
}

Describe 'Export-Settings' {
    BeforeAll {
        $script:testRoot = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-export-tests-$([guid]::NewGuid().ToString('N'))"
        New-Item -Path $script:testRoot -ItemType Directory -Force | Out-Null
    }

    AfterAll {
        if (Test-Path $script:testRoot) {
            Remove-Item -Path $script:testRoot -Recurse -Force
        }
    }

    It 'writes JSON to settings path and ensures downloads directory' {
        $settingsDir = Join-Path $script:testRoot 'export'
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
        $settingsPath = Join-Path $settingsDir 'settings.json'

        $settings = [PSCustomObject]@{
            downloadsDirectory = 'C:/tmp/configurator-downloads'
            manifest = [PSCustomObject]@{
                repo = $null
                fileName = 'manifest.json'
                directory = $null
            }
            git = [PSCustomObject]@{
                cloneDirectory = 'C:\src\'
            }
        }

        InModuleScope Configurator -Parameters @{ settingsPath = $settingsPath; settings = $settings } {
            param($settingsPath, $settings)
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            Export-Settings -Settings $settings

            Test-Path $settingsPath | Should -BeTrue
            $written = Get-Content -Path $settingsPath -Raw | ConvertFrom-Json
            $written.downloadsDirectory | Should -Be 'C:/tmp/configurator-downloads'
            $written.manifest.fileName | Should -Be 'manifest.json'
        }
    }

    It 'round-trips: save then load returns same values' {
        $settingsDir = Join-Path $script:testRoot 'roundtrip'
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
        $settingsPath = Join-Path $settingsDir 'settings.json'

        $settings = [PSCustomObject]@{
            downloadsDirectory = 'C:/tmp/configurator-downloads'
            manifest = [PSCustomObject]@{
                repo = 'https://github.com/test/repo.git'
                fileName = 'my-manifest.json'
                directory = 'C:\repos\test'
            }
            git = [PSCustomObject]@{
                cloneDirectory = 'C:\code\'
            }
        }

        InModuleScope Configurator -Parameters @{ settingsPath = $settingsPath; settings = $settings } {
            param($settingsPath, $settings)
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            Export-Settings -Settings $settings
            $loaded = Import-Settings

            $loaded.downloadsDirectory | Should -Be $settings.downloadsDirectory
            $loaded.manifest.repo | Should -Be $settings.manifest.repo
            $loaded.manifest.fileName | Should -Be $settings.manifest.fileName
            $loaded.manifest.directory | Should -Be $settings.manifest.directory
            $loaded.git.cloneDirectory | Should -Be $settings.git.cloneDirectory
        }
    }
}
