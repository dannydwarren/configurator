BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Invoke-Initialize' {
    It 'runs system initializer, sets manifest directory, clones repo, creates manifest file' {
        InModuleScope Configurator {
            $testRoot = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-init-test-$([guid]::NewGuid().ToString('N'))"
            $cloneDir = Join-Path $testRoot 'src'
            New-Item -Path $cloneDir -ItemType Directory -Force | Out-Null

            $settingsDir = Join-Path $testRoot 'settings'
            New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
            $settingsPath = Join-Path $settingsDir 'settings.json'

            Mock Initialize-System {}
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            $mockSettings = [PSCustomObject]@{
                downloadsDirectory = 'C:/tmp/configurator-downloads'
                manifest = [PSCustomObject]@{
                    repo      = 'https://github.com/user/machine-configs.git'
                    fileName  = 'manifest.json'
                    directory = $null
                }
                git = [PSCustomObject]@{
                    cloneDirectory = $cloneDir
                }
            }
            Mock Import-Settings { return $mockSettings }
            Mock Export-Settings {}
            Mock Test-Path { return $false }
            Mock Invoke-PowerShellScript {}
            Mock Set-Content {}

            Invoke-Initialize

            Should -Invoke Initialize-System -Times 1
            Should -Invoke Export-Settings -Times 1
            Should -Invoke Invoke-PowerShellScript -Times 2

            $mockSettings.manifest.directory | Should -Not -BeNullOrEmpty

            try { Remove-Item -Path $testRoot -Recurse -Force } catch {}
        }
    }

    It 'creates and commits manifest file when it does not exist' {
        InModuleScope Configurator {
            $testRoot = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-init-newmanifest-$([guid]::NewGuid().ToString('N'))"
            $cloneDir = Join-Path $testRoot 'src'
            $manifestDir = Join-Path $cloneDir 'machine-configs'
            New-Item -Path $manifestDir -ItemType Directory -Force | Out-Null

            $settingsDir = Join-Path $testRoot 'settings'
            New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
            $settingsPath = Join-Path $settingsDir 'settings.json'

            Mock Initialize-System {}
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            $mockSettings = [PSCustomObject]@{
                downloadsDirectory = 'C:/tmp/configurator-downloads'
                manifest = [PSCustomObject]@{
                    repo      = 'https://github.com/user/machine-configs.git'
                    fileName  = 'manifest.json'
                    directory = $manifestDir
                }
                git = [PSCustomObject]@{
                    cloneDirectory = $cloneDir
                }
            }
            Mock Import-Settings { return $mockSettings }
            Mock Export-Settings {}
            Mock Test-Path { return $true } -ParameterFilter { $Path -eq $manifestDir }
            Mock Test-Path { return $false } -ParameterFilter { $Path -ne $manifestDir }
            Mock Invoke-PowerShellScript {}
            Mock Set-Content {}

            Invoke-Initialize

            Should -Invoke Invoke-PowerShellScript -Times 1 -ParameterFilter {
                $Script -like '*git add*' -and $Script -like '*git commit*' -and $Script -like '*git push*'
            }
            Should -Invoke Set-Content -Times 1

            try { Remove-Item -Path $testRoot -Recurse -Force } catch {}
        }
    }

    It 'does nothing when manifest directory and file already exist' {
        InModuleScope Configurator {
            $testRoot = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-init-exists-$([guid]::NewGuid().ToString('N'))"
            $cloneDir = Join-Path $testRoot 'src'
            $manifestDir = Join-Path $cloneDir 'machine-configs'
            New-Item -Path $manifestDir -ItemType Directory -Force | Out-Null

            $settingsDir = Join-Path $testRoot 'settings'
            New-Item -Path $settingsDir -ItemType Directory -Force | Out-Null
            $settingsPath = Join-Path $settingsDir 'settings.json'

            Mock Initialize-System {}
            Mock Get-SettingsPath { return $settingsPath }
            Mock New-Item {}

            $mockSettings = [PSCustomObject]@{
                downloadsDirectory = 'C:/tmp/configurator-downloads'
                manifest = [PSCustomObject]@{
                    repo      = 'https://github.com/user/machine-configs.git'
                    fileName  = 'manifest.json'
                    directory = $manifestDir
                }
                git = [PSCustomObject]@{
                    cloneDirectory = $cloneDir
                }
            }
            Mock Import-Settings { return $mockSettings }
            Mock Export-Settings {}
            Mock Test-Path { return $true }
            Mock Invoke-PowerShellScript {}
            Mock Set-Content {}

            Invoke-Initialize

            Should -Invoke Invoke-PowerShellScript -Times 0
            Should -Invoke Set-Content -Times 0

            try { Remove-Item -Path $testRoot -Recurse -Force } catch {}
        }
    }

    It 'throws if manifest.repo setting is null' {
        InModuleScope Configurator {
            Mock Initialize-System {}
            Mock New-Item {}

            $mockSettings = [PSCustomObject]@{
                downloadsDirectory = 'C:/tmp/configurator-downloads'
                manifest = [PSCustomObject]@{
                    repo      = $null
                    fileName  = 'manifest.json'
                    directory = $null
                }
                git = [PSCustomObject]@{
                    cloneDirectory = 'C:\src\'
                }
            }
            Mock Import-Settings { return $mockSettings }

            { Invoke-Initialize } | Should -Throw "*manifest.repo*must be set*"
        }
    }
}
