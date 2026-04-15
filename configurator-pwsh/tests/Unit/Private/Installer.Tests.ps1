BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Install-App' {
    It 'installs when app is not yet installed' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = 'verify-script'
                UpgradeScript      = $null
                PreventUpgrade     = $false
            }

            $script:verifyCallCount = 0
            Mock Invoke-PowerShellScript {
                if ($ResultType -eq [bool]) {
                    $script:verifyCallCount++
                    if ($script:verifyCallCount -eq 1) { return $false }
                    return $true
                }
            }
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app

            Should -Invoke Invoke-PowerShellScript -Times 3 -Exactly
            Should -Invoke Write-ConfiguratorLog -ParameterFilter { $Message -eq "Installing 'test-app'" }
            Should -Invoke Write-ConfiguratorLog -ParameterFilter { $Message -eq "Installed 'test-app'" }
        }
    }

    It 'uses Windows PowerShell when ForceWindowsPowerShell is set' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = 'verify-script'
                UpgradeScript      = $null
                PreventUpgrade     = $false
            }

            Mock Invoke-WindowsPowerShellScript {
                if ($ResultType -eq [bool]) { return $false }
            }
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app -ForceWindowsPowerShell

            Should -Invoke Invoke-WindowsPowerShellScript -Times 3 -Exactly
        }
    }

    It 'skips verification when no verification script' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
            }

            Mock Invoke-PowerShellScript {}
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app

            Should -Invoke Invoke-PowerShellScript -Times 1 -Exactly -ParameterFilter { $Script -eq 'install-script' }
        }
    }

    It 'does not call Remove-DesktopShortcuts when nothing added to desktop' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
            }

            Mock Invoke-PowerShellScript {}
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app

            Should -Invoke Remove-DesktopShortcuts -Times 1 -Exactly -ParameterFilter {
                $BeforeEntries.Count -eq 0 -and $AfterEntries.Count -eq 0
            }
        }
    }

    It 'upgrades when already installed' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = 'verify-script'
                UpgradeScript      = 'upgrade-script'
                PreventUpgrade     = $false
            }

            Mock Invoke-PowerShellScript {
                if ($ResultType -eq [bool]) { return $true }
            }
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app

            Should -Invoke Invoke-PowerShellScript -ParameterFilter { $Script -eq 'upgrade-script' }
        }
    }

    It 'prevents upgrade when PreventUpgrade is true' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = 'verify-script'
                UpgradeScript      = 'upgrade-script'
                PreventUpgrade     = $true
            }

            Mock Invoke-PowerShellScript {
                if ($ResultType -eq [bool]) { return $true }
            }
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app

            Should -Not -Invoke Invoke-PowerShellScript -ParameterFilter { $Script -eq 'upgrade-script' }
            Should -Not -Invoke Invoke-PowerShellScript -ParameterFilter { $Script -eq 'install-script' }
        }
    }

    It 'does nothing when already installed with no upgrade script' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = 'verify-script'
                UpgradeScript      = $null
                PreventUpgrade     = $false
            }

            Mock Invoke-PowerShellScript {
                if ($ResultType -eq [bool]) { return $true }
            }
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app

            Should -Not -Invoke Invoke-PowerShellScript -ParameterFilter { $Script -eq 'install-script' }
        }
    }

    It 'logs debug when post-install verification fails' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'test-app'
                InstallScript      = 'install-script'
                VerificationScript = 'verify-script'
                UpgradeScript      = $null
                PreventUpgrade     = $false
            }

            Mock Invoke-PowerShellScript {
                if ($ResultType -eq [bool]) { return $false }
            }
            Mock Get-DesktopEntries { @() }
            Mock Remove-DesktopShortcuts {}
            Mock Write-ConfiguratorLog {}

            Install-App -App $app

            Should -Invoke Write-ConfiguratorLog -ParameterFilter {
                $Message -eq "Failed to install 'test-app'" -and $Level -eq 'Debug'
            }
        }
    }
}

Describe 'Install-DownloadApp' {
    It 'downloads and delegates to Install-App' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'download-app'
                AppType            = 'gitHubAsset'
                Downloader         = 'GitHubAssetDownloader'
                DownloaderArgs     = [PSCustomObject]@{ User = 'user'; Repo = 'repo'; Extension = '.exe' }
                DownloadedFilePath = $null
                InstallScript      = 'install'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
            }

            Mock Get-GitHubAsset { return 'C:\downloads\file.exe' }
            Mock Install-App {}
            Mock Write-ConfiguratorLog {}

            Install-DownloadApp -App $app

            $app.DownloadedFilePath | Should -Be 'C:\downloads\file.exe'
            Should -Invoke Write-ConfiguratorLog -ParameterFilter { $Message -eq "Downloading 'download-app'" }
            Should -Invoke Write-ConfiguratorLog -ParameterFilter { $Message -eq "Downloaded 'download-app'" }
            Should -Invoke Install-App -Times 1 -Exactly
        }
    }
}

Describe 'Invoke-AppConfigurator' {
    Context 'Configure' {
        It 'sets registry values from configuration' {
            InModuleScope Configurator {
                $app = [PSCustomObject]@{
                    AppId         = 'config-app'
                    Configuration = [PSCustomObject]@{
                        registrySettings = @(
                            [PSCustomObject]@{ keyName = 'HKCU\Test'; valueName = 'Setting1'; valueData = 'Value1' }
                            [PSCustomObject]@{ keyName = 'HKCU\Test'; valueName = 'Setting2'; valueData = 'Value2' }
                        )
                    }
                }

                Mock Set-RegistryValue {}

                Invoke-AppConfigurator -App $app -Configure

                Should -Invoke Set-RegistryValue -Times 2 -Exactly
            }
        }

        It 'does nothing when configuration is null' {
            InModuleScope Configurator {
                $app = [PSCustomObject]@{
                    AppId         = 'no-config-app'
                    Configuration = $null
                }

                Mock Set-RegistryValue {}

                Invoke-AppConfigurator -App $app -Configure

                Should -Not -Invoke Set-RegistryValue
            }
        }
    }

    Context 'Backup' {
        It 'backs up an installed app with backup script' {
            InModuleScope Configurator {
                $testDir = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-backup-test-$([guid]::NewGuid().ToString('N'))"
                $appDir = Join-Path $testDir 'apps' 'backup-app'
                New-Item -Path $appDir -ItemType Directory -Force | Out-Null
                Set-Content -Path (Join-Path $appDir 'backup.ps1') -Value 'Write-Host "backup"'

                $app = [PSCustomObject]@{
                    AppId              = 'backup-app'
                    VerificationScript = 'verify-script'
                }

                Mock Invoke-PowerShellScript {
                    if ($ResultType -eq [bool]) { return $true }
                }
                Mock Import-Settings {
                    [PSCustomObject]@{
                        manifest = [PSCustomObject]@{ directory = $testDir }
                    }
                }
                Mock Write-ConfiguratorLog {}

                Invoke-AppConfigurator -App $app -Backup

                Should -Invoke Write-ConfiguratorLog -ParameterFilter { $Message -eq 'Backing up backup-app...' }
                Should -Invoke Write-ConfiguratorLog -ParameterFilter { $Message -eq 'Backed up backup-app!' }

                Remove-Item -Path $testDir -Recurse -Force
            }
        }

        It 'skips when no verification script' {
            InModuleScope Configurator {
                $app = [PSCustomObject]@{
                    AppId              = 'no-verify-app'
                    VerificationScript = $null
                }

                Mock Invoke-PowerShellScript {}
                Mock Import-Settings {}
                Mock Write-ConfiguratorLog {}

                Invoke-AppConfigurator -App $app -Backup

                Should -Not -Invoke Invoke-PowerShellScript
            }
        }

        It 'skips when app is not installed' {
            InModuleScope Configurator {
                $app = [PSCustomObject]@{
                    AppId              = 'not-installed-app'
                    VerificationScript = 'verify-script'
                }

                Mock Invoke-PowerShellScript {
                    if ($ResultType -eq [bool]) { return $false }
                }
                Mock Import-Settings {}
                Mock Write-ConfiguratorLog {}

                Invoke-AppConfigurator -App $app -Backup

                Should -Not -Invoke Import-Settings
            }
        }

        It 'skips when backup script does not exist' {
            InModuleScope Configurator {
                $testDir = Join-Path ([System.IO.Path]::GetTempPath()) "configurator-backup-test-$([guid]::NewGuid().ToString('N'))"
                New-Item -Path $testDir -ItemType Directory -Force | Out-Null

                $app = [PSCustomObject]@{
                    AppId              = 'no-backup-script-app'
                    VerificationScript = 'verify-script'
                }

                Mock Invoke-PowerShellScript {
                    if ($ResultType -eq [bool]) { return $true }
                }
                Mock Import-Settings {
                    [PSCustomObject]@{
                        manifest = [PSCustomObject]@{ directory = $testDir }
                    }
                }
                Mock Write-ConfiguratorLog {}

                Invoke-AppConfigurator -App $app -Backup

                Should -Not -Invoke Write-ConfiguratorLog -ParameterFilter { $Message -like 'Backing up*' }

                Remove-Item -Path $testDir -Recurse -Force
            }
        }
    }
}
