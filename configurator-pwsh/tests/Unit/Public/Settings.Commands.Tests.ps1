BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Get-ConfiguratorSettings' {
    It 'lists all setting paths with correct names and types' {
        InModuleScope Configurator {
            $mockSettings = [PSCustomObject]@{
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
            Mock Import-Settings { return $mockSettings }

            $rows = Get-SettingRows -Node $mockSettings -Prefix ''

            $rows.Count | Should -Be 5

            $dlRow = $rows | Where-Object { $_.Name -eq 'downloadsdirectory' }
            $dlRow.Value | Should -Be 'C:/tmp/configurator-downloads'
            $dlRow.Type | Should -Be 'Uri'

            $repoRow = $rows | Where-Object { $_.Name -eq 'manifest.repo' }
            $repoRow.Value | Should -Be ''
            $repoRow.Type | Should -Be 'Uri'

            $fileRow = $rows | Where-Object { $_.Name -eq 'manifest.filename' }
            $fileRow.Value | Should -Be 'manifest.json'
            $fileRow.Type | Should -Be 'String'

            $dirRow = $rows | Where-Object { $_.Name -eq 'manifest.directory' }
            $dirRow.Value | Should -Be ''
            $dirRow.Type | Should -Be 'String'

            $gitRow = $rows | Where-Object { $_.Name -eq 'git.clonedirectory' }
            $gitRow.Value | Should -Be 'C:\src\'
            $gitRow.Type | Should -Be 'Uri'
        }
    }
}

Describe 'Set-ConfiguratorSetting' {
    BeforeAll {
        $script:testRoot = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-setcmd-tests-$([guid]::NewGuid().ToString('N'))"
        New-Item -Path $script:testRoot -ItemType Directory -Force | Out-Null
    }

    AfterAll {
        if (Test-Path $script:testRoot) {
            Remove-Item -Path $script:testRoot -Recurse -Force
        }
    }

    It 'updates manifest.repo with Uri value' {
        $settingsDir = Join-Path $script:testRoot 'set-repo'
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
        $settingsPath = Join-Path $settingsDir 'settings.json'

        InModuleScope Configurator -Parameters @{ settingsPath = $settingsPath } {
            param($settingsPath)
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            $mockSettings = [PSCustomObject]@{
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
            Mock Import-Settings { return $mockSettings }

            $exportedSettings = $null
            Mock Export-Settings { $script:exportedSettings = $Settings }

            Set-ConfiguratorSetting -Name 'manifest.repo' -Value 'https://github.com/user/repo.git'

            $mockSettings.manifest.repo | Should -Not -BeNullOrEmpty
            $mockSettings.manifest.repo.ToString() | Should -Be 'https://github.com/user/repo.git'
            $mockSettings.manifest.repo | Should -BeOfType [uri]
        }
    }

    It 'updates manifest.filename with string value' {
        $settingsDir = Join-Path $script:testRoot 'set-filename'
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
        $settingsPath = Join-Path $settingsDir 'settings.json'

        InModuleScope Configurator -Parameters @{ settingsPath = $settingsPath } {
            param($settingsPath)
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            $mockSettings = [PSCustomObject]@{
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
            Mock Import-Settings { return $mockSettings }
            Mock Export-Settings {}

            Set-ConfiguratorSetting -Name 'manifest.filename' -Value 'custom.json'

            $mockSettings.manifest.fileName | Should -Be 'custom.json'
        }
    }

    It 'throws for unknown setting name' {
        InModuleScope Configurator {
            $mockSettings = [PSCustomObject]@{
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
            Mock Import-Settings { return $mockSettings }
            Mock Export-Settings {}

            { Set-ConfiguratorSetting -Name 'nonexistent.setting' -Value 'value' } | Should -Throw '*is not a recognized setting name*'
        }
    }

    It 'persists value through save and reload' {
        $settingsDir = Join-Path $script:testRoot 'set-persist'
        New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
        $settingsPath = Join-Path $settingsDir 'settings.json'

        InModuleScope Configurator -Parameters @{ settingsPath = $settingsPath } {
            param($settingsPath)
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            Set-ConfiguratorSetting -Name 'manifest.filename' -Value 'updated.json'

            $loaded = Import-Settings
            $loaded.manifest.fileName | Should -Be 'updated.json'
        }
    }
}
