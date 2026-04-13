BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Invoke-Backup' {
    It 'loads full manifest and calls backup for each app' {
        InModuleScope Configurator {
            $app1 = [PSCustomObject]@{
                AppId              = 'app-1'
                VerificationScript = 'verify-1'
            }
            $app2 = [PSCustomObject]@{
                AppId              = 'app-2'
                VerificationScript = 'verify-2'
            }

            Mock Import-Manifest {
                @{
                    AppIds = @('app-1', 'app-2')
                    Apps   = @($app1, $app2)
                }
            }
            Mock Invoke-AppConfigurator {}

            Invoke-Backup

            Should -Invoke Import-Manifest -Times 1 -Exactly -ParameterFilter {
                $Environments.Count -eq 0
            }
            Should -Invoke Invoke-AppConfigurator -Times 2 -Exactly
            Should -Invoke Invoke-AppConfigurator -ParameterFilter {
                $App.AppId -eq 'app-1' -and $Backup -eq $true
            }
            Should -Invoke Invoke-AppConfigurator -ParameterFilter {
                $App.AppId -eq 'app-2' -and $Backup -eq $true
            }
        }
    }

    It 'handles manifest with no apps' {
        InModuleScope Configurator {
            Mock Import-Manifest {
                @{
                    AppIds = @()
                    Apps   = @()
                }
            }
            Mock Invoke-AppConfigurator {}

            Invoke-Backup

            Should -Not -Invoke Invoke-AppConfigurator
        }
    }
}
