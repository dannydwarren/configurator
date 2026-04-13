BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Initialize-System' {
    It 'executes all 9 initialization steps in order' {
        InModuleScope Configurator {
            $script:callOrder = @()
            Mock Set-PowerShellPolicy { $script:callOrder += "SetPolicy-$Edition" }
            Mock Set-WingetConfiguration { $script:callOrder += "Winget-$Action" }
            Mock Install-PowerShellCore { $script:callOrder += 'InstallPSCore' }
            Mock Install-Self { $script:callOrder += 'InstallSelf' }
            Mock Install-ScoopCli { $script:callOrder += 'InstallScoop' }
            Mock Install-Git { $script:callOrder += 'InstallGit' }
            Mock Install-ManifestRepo { $script:callOrder += 'InstallManifestRepo' }

            Initialize-System

            $script:callOrder.Count | Should -Be 9
            $script:callOrder[0] | Should -Be 'SetPolicy-Windows'
            $script:callOrder[1] | Should -Be 'Winget-Upgrade'
            $script:callOrder[2] | Should -Be 'Winget-AcceptSourceAgreements'
            $script:callOrder[3] | Should -Be 'InstallPSCore'
            $script:callOrder[4] | Should -Be 'SetPolicy-Core'
            $script:callOrder[5] | Should -Be 'InstallSelf'
            $script:callOrder[6] | Should -Be 'InstallScoop'
            $script:callOrder[7] | Should -Be 'InstallGit'
            $script:callOrder[8] | Should -Be 'InstallManifestRepo'
        }
    }
}

Describe 'Set-PowerShellPolicy' {
    It 'sets PS Core execution policy via admin and reports policy and version' {
        InModuleScope Configurator {
            Mock Invoke-PowerShellScript {}
            Mock Invoke-PowerShellScript { return 'RemoteSigned' } -ParameterFilter { $null -ne $ResultType -and $Script -eq 'Get-ExecutionPolicy' }
            Mock Invoke-PowerShellScript { return '7.4.0' } -ParameterFilter { $null -ne $ResultType -and $Script -eq '$PSVersionTable.PSVersion.ToString()' }
            Mock Write-ConfiguratorLog {}

            Set-PowerShellPolicy -Edition Core

            Should -Invoke Invoke-PowerShellScript -ParameterFilter { $RunAsAdmin -eq $true -and $Script -eq 'Set-ExecutionPolicy RemoteSigned -Force' } -Times 1
            Should -Invoke Invoke-PowerShellScript -ParameterFilter { $null -ne $ResultType -and $Script -eq 'Get-ExecutionPolicy' } -Times 1
            Should -Invoke Invoke-PowerShellScript -ParameterFilter { $null -ne $ResultType -and $Script -eq '$PSVersionTable.PSVersion.ToString()' } -Times 1
        }
    }

    It 'sets Windows PowerShell execution policy via admin and reports policy and version' {
        InModuleScope Configurator {
            Mock Invoke-WindowsPowerShellScript {}
            Mock Invoke-WindowsPowerShellScript { return 'RemoteSigned' } -ParameterFilter { $null -ne $ResultType -and $Script -eq 'Get-ExecutionPolicy' }
            Mock Invoke-WindowsPowerShellScript { return '5.1.0' } -ParameterFilter { $null -ne $ResultType -and $Script -eq '$PSVersionTable.PSVersion.ToString()' }
            Mock Write-ConfiguratorLog {}

            Set-PowerShellPolicy -Edition Windows

            Should -Invoke Invoke-WindowsPowerShellScript -ParameterFilter { $RunAsAdmin -eq $true -and $Script -eq 'Set-ExecutionPolicy RemoteSigned -Force' } -Times 1
            Should -Invoke Invoke-WindowsPowerShellScript -ParameterFilter { $null -ne $ResultType -and $Script -eq 'Get-ExecutionPolicy' } -Times 1
            Should -Invoke Invoke-WindowsPowerShellScript -ParameterFilter { $null -ne $ResultType -and $Script -eq '$PSVersionTable.PSVersion.ToString()' } -Times 1
        }
    }
}

Describe 'Set-WingetConfiguration' {
    It 'executes two Add-AppxPackage commands for Upgrade' {
        InModuleScope Configurator {
            $script:winScripts = @()
            Mock Invoke-WindowsPowerShellScript { $script:winScripts += $Script }

            Set-WingetConfiguration -Action Upgrade

            $script:winScripts.Count | Should -Be 2
            $script:winScripts[0] | Should -BeLike '*Microsoft.DesktopAppInstaller*msixbundle*'
            $script:winScripts[1] | Should -BeLike '*source.msix*'
        }
    }

    It 'executes winget list with accept-source-agreements for AcceptSourceAgreements' {
        InModuleScope Configurator {
            $script:winScripts = @()
            Mock Invoke-WindowsPowerShellScript { $script:winScripts += $Script }

            Set-WingetConfiguration -Action AcceptSourceAgreements

            $script:winScripts.Count | Should -Be 1
            $script:winScripts[0] | Should -BeLike '*winget list*--accept-source-agreements*'
        }
    }
}

Describe 'Install-PowerShellCore' {
    It 'installs Microsoft.PowerShell WingetApp via ForceWindowsPowerShell' {
        InModuleScope Configurator {
            Mock Install-App {}
            Mock Get-DesktopEntries { return @() }
            Mock Remove-DesktopShortcuts {}

            Install-PowerShellCore

            Should -Invoke Install-App -Times 1 -ParameterFilter {
                $App.AppId -eq 'Microsoft.PowerShell' -and
                $App.AppType -eq 'winget' -and
                $ForceWindowsPowerShell -eq $true
            }
        }
    }
}

Describe 'Install-Self' {
    It 'downloads Configurator from GitHub, moves to install dir, and adds to PATH' {
        InModuleScope Configurator {
            Mock Test-Path { return $false } -ParameterFilter { $Path -eq 'c:\Configurator' }
            Mock Install-DownloadApp {
                $App.DownloadedFilePath = 'C:\tmp\Configurator.exe'
            }
            Mock New-Item {}
            Mock Move-Item {}
            Mock Add-ToMachinePath {}
            Mock Get-DesktopEntries { return @() }
            Mock Remove-DesktopShortcuts {}

            Install-Self

            Should -Invoke Install-DownloadApp -Times 1 -ParameterFilter {
                $App.AppId -eq 'Configurator' -and
                $App.IsDownloadApp -eq $true
            }
            Should -Invoke New-Item -Times 1 -ParameterFilter { $Path -eq 'c:\Configurator' }
            Should -Invoke Move-Item -Times 1
            Should -Invoke Add-ToMachinePath -Times 1 -ParameterFilter { $Directory -eq 'c:\Configurator' }
        }
    }

    It 'does nothing if install directory already exists' {
        InModuleScope Configurator {
            Mock Test-Path { return $true } -ParameterFilter { $Path -eq 'c:\Configurator' }
            Mock Install-DownloadApp {}

            Install-Self

            Should -Invoke Install-DownloadApp -Times 0
        }
    }
}

Describe 'Install-ScoopCli' {
    It 'installs ScriptApp with scoop install and verification scripts' {
        InModuleScope Configurator {
            Mock Install-App {}
            Mock Get-DesktopEntries { return @() }
            Mock Remove-DesktopShortcuts {}

            Install-ScoopCli

            Should -Invoke Install-App -Times 1 -ParameterFilter {
                $App.AppId -eq 'ScoopCli' -and
                $App.AppType -eq 'script' -and
                $App.InstallScript -like '*get.scoop.sh*' -and
                $App.VerificationScript -like '*Test-CommandExists scoop*'
            }
        }
    }
}

Describe 'Install-Git' {
    It 'installs Git.Git via Winget' {
        InModuleScope Configurator {
            Mock Install-App {}
            Mock Get-DesktopEntries { return @() }
            Mock Remove-DesktopShortcuts {}

            Install-Git

            Should -Invoke Install-App -Times 1 -ParameterFilter {
                $App.AppId -eq 'Git.Git' -and
                $App.AppType -eq 'winget'
            }
        }
    }
}

Describe 'Install-ManifestRepo' {
    It 'installs git repo with manifest repo URL from settings' {
        InModuleScope Configurator {
            Mock Import-Settings {
                return [PSCustomObject]@{
                    downloadsDirectory = 'C:/tmp/configurator-downloads'
                    manifest = [PSCustomObject]@{
                        repo      = 'https://github.com/user/machine-configs.git'
                        fileName  = 'manifest.json'
                        directory = $null
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }
            Mock Install-App {}
            Mock Get-DesktopEntries { return @() }
            Mock Remove-DesktopShortcuts {}

            Install-ManifestRepo

            Should -Invoke Install-App -Times 1 -ParameterFilter {
                $App.AppId -eq 'git.manifest-repo' -and
                $App.AppType -eq 'gitRepo' -and
                $App.InstallArgs -eq 'https://github.com/user/machine-configs.git'
            }
        }
    }

    It 'throws if manifest.repo setting is null' {
        InModuleScope Configurator {
            Mock Import-Settings {
                return [PSCustomObject]@{
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
            }

            { Install-ManifestRepo } | Should -Throw '*Missing setting: Manifest.Repo*'
        }
    }
}
