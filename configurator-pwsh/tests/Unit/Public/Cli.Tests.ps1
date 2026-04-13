BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Start-Configurator' {
    Context 'Privilege check' {
        It 'returns exit code 2 when not elevated' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $false }
                Mock Write-ConfiguratorLog {}

                $result = Start-Configurator -Command 'initialize'

                $result | Should -Be 2
                Should -Invoke Write-ConfiguratorLog -ParameterFilter {
                    $Message -eq 'Configurator Cli must be run with elevated privileges.' -and $Level -eq 'Error'
                }
            }
        }
    }

    Context 'No arguments' {
        It 'shows help and returns exit code 0' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Write-Host {}

                $result = Start-Configurator

                $result | Should -Be 0
                Should -Invoke Write-Host -Times 1 -Exactly
            }
        }
    }

    Context 'Invalid command' {
        It 'returns exit code 1 for unknown command' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Write-ConfiguratorLog {}

                $result = Start-Configurator -Command 'invalid-arg'

                $result | Should -Be 1
                Should -Invoke Write-ConfiguratorLog -ParameterFilter {
                    $Message -eq 'Unknown command: invalid-arg' -and $Level -eq 'Error'
                }
            }
        }
    }

    Context 'initialize command' {
        It 'executes initialize and returns 0' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-Initialize {}

                $result = Start-Configurator -Command 'initialize'

                $result | Should -Be 0
                Should -Invoke Invoke-Initialize -Times 1 -Exactly
            }
        }
    }

    Context 'settings list command' {
        It 'executes list settings and returns 0' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Get-ConfiguratorSettings {}

                $result = Start-Configurator -Command 'settings' -RemainingArgs @('list')

                $result | Should -Be 0
                Should -Invoke Get-ConfiguratorSettings -Times 1 -Exactly
            }
        }
    }

    Context 'settings set command' {
        It 'executes set setting with both args and returns 0' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Set-ConfiguratorSetting {}

                $result = Start-Configurator -Command 'settings' -RemainingArgs @('set', 'manifest.repo', 'https://github.com/user/repo.git')

                $result | Should -Be 0
                Should -Invoke Set-ConfiguratorSetting -Times 1 -Exactly -ParameterFilter {
                    $Name -eq 'manifest.repo' -and $Value -eq 'https://github.com/user/repo.git'
                }
            }
        }

        It 'returns exit code 1 when missing arguments' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Write-ConfiguratorLog {}

                $result = Start-Configurator -Command 'settings' -RemainingArgs @('set', 'manifest.repo')

                $result | Should -Be 1
            }
        }
    }

    Context 'settings with no subcommand' {
        It 'returns exit code 1' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Write-Host {}

                $result = Start-Configurator -Command 'settings'

                $result | Should -Be 1
            }
        }
    }

    Context 'add-app command' {
        It 'parses pipe-separated environments and executes add-app' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Add-ConfiguratorApp {}

                $result = Start-Configurator -Command 'add-app' -RemainingArgs @(
                    '--app-id', 'my-app',
                    '--app-type', 'winget',
                    '--environments', 'Work|Personal'
                )

                $result | Should -Be 0
                Should -Invoke Add-ConfiguratorApp -Times 1 -Exactly -ParameterFilter {
                    $AppId -eq 'my-app' -and $AppType -eq 'winget'
                }
            }
        }

        It 'returns exit code 1 when missing required args' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Write-ConfiguratorLog {}

                $result = Start-Configurator -Command 'add-app' -RemainingArgs @('--app-id', 'my-app')

                $result | Should -Be 1
            }
        }
    }

    Context 'add command (alias)' {
        It 'works the same as add-app' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Add-ConfiguratorApp {}

                $result = Start-Configurator -Command 'add' -RemainingArgs @(
                    '--app-id', 'my-app',
                    '--app-type', 'scoop',
                    '--environments', 'Dev'
                )

                $result | Should -Be 0
                Should -Invoke Add-ConfiguratorApp -Times 1 -Exactly
            }
        }
    }

    Context 'configure-machine command' {
        It 'executes with empty environment list and no single app' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-ConfigureMachine {}

                $result = Start-Configurator -Command 'configure-machine'

                $result | Should -Be 0
                Should -Invoke Invoke-ConfigureMachine -Times 1 -Exactly
            }
        }
    }

    Context 'configure command (alias)' {
        It 'works the same as configure-machine' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-ConfigureMachine {}

                $result = Start-Configurator -Command 'configure'

                $result | Should -Be 0
                Should -Invoke Invoke-ConfigureMachine -Times 1 -Exactly
            }
        }
    }

    Context 'configure --single-app-id' {
        It 'passes the single app ID' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-ConfigureMachine {}

                $result = Start-Configurator -Command 'configure' -RemainingArgs @('--single-app-id', 'my-app')

                $result | Should -Be 0
                Should -Invoke Invoke-ConfigureMachine -Times 1 -Exactly -ParameterFilter {
                    $SingleAppId -eq 'my-app'
                }
            }
        }
    }

    Context 'configure -app (alias)' {
        It 'passes the single app ID via -app alias' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-ConfigureMachine {}

                $result = Start-Configurator -Command 'configure' -RemainingArgs @('-app', 'my-app')

                $result | Should -Be 0
                Should -Invoke Invoke-ConfigureMachine -Times 1 -Exactly -ParameterFilter {
                    $SingleAppId -eq 'my-app'
                }
            }
        }
    }

    Context 'configure --environments' {
        It 'parses pipe-separated environments' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-ConfigureMachine {}

                $result = Start-Configurator -Command 'configure' -RemainingArgs @('--environments', 'Work|Personal')

                $result | Should -Be 0
                Should -Invoke Invoke-ConfigureMachine -Times 1 -Exactly -ParameterFilter {
                    $Environments.Count -eq 2 -and $Environments[0] -eq 'Work' -and $Environments[1] -eq 'Personal'
                }
            }
        }
    }

    Context 'configure -e (alias)' {
        It 'parses environments via -e alias' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-ConfigureMachine {}

                $result = Start-Configurator -Command 'configure' -RemainingArgs @('-e', 'Dev|Staging')

                $result | Should -Be 0
                Should -Invoke Invoke-ConfigureMachine -Times 1 -Exactly -ParameterFilter {
                    $Environments.Count -eq 2 -and $Environments[0] -eq 'Dev' -and $Environments[1] -eq 'Staging'
                }
            }
        }
    }

    Context 'backup command' {
        It 'executes backup and returns 0' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-Backup {}

                $result = Start-Configurator -Command 'backup'

                $result | Should -Be 0
                Should -Invoke Invoke-Backup -Times 1 -Exactly
            }
        }
    }

    Context 'command throws exception' {
        It 'catches exception and returns exit code 1' {
            InModuleScope Configurator {
                Mock Test-Administrator { return $true }
                Mock Invoke-Initialize { throw 'Something went wrong' }
                Mock Write-ConfiguratorLog {}

                $result = Start-Configurator -Command 'initialize'

                $result | Should -Be 1
                Should -Invoke Write-ConfiguratorLog -ParameterFilter {
                    $Message -eq 'Something went wrong' -and $Level -eq 'Error'
                }
            }
        }
    }
}

Describe 'Get-NamedArgValue' {
    It 'extracts value for matching arg name' {
        InModuleScope Configurator {
            $result = Get-NamedArgValue -ArgList @('--name', 'value') -Names @('--name')
            $result | Should -Be 'value'
        }
    }

    It 'returns null when arg not found' {
        InModuleScope Configurator {
            $result = Get-NamedArgValue -ArgList @('--other', 'value') -Names @('--name')
            $result | Should -BeNullOrEmpty
        }
    }

    It 'handles null arg list' {
        InModuleScope Configurator {
            $result = Get-NamedArgValue -ArgList $null -Names @('--name')
            $result | Should -BeNullOrEmpty
        }
    }

    It 'matches first of multiple aliases' {
        InModuleScope Configurator {
            $result = Get-NamedArgValue -ArgList @('-e', 'envs') -Names @('--environments', '-e')
            $result | Should -Be 'envs'
        }
    }
}
