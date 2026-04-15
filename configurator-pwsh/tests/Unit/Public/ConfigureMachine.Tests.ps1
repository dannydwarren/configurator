BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Invoke-ConfigureMachine' {
    It 'installs all manifest apps and configures each' {
        InModuleScope Configurator {
            $app1 = [PSCustomObject]@{
                AppId              = 'app-1'
                AppType            = 'winget'
                IsDownloadApp      = $false
                InstallScript      = 'install-1'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
                Configuration      = $null
            }
            $app2 = [PSCustomObject]@{
                AppId              = 'app-2'
                AppType            = 'scoop'
                IsDownloadApp      = $false
                InstallScript      = 'install-2'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
                Configuration      = $null
            }

            Mock Import-Manifest {
                @{
                    AppIds = @('app-1', 'app-2')
                    Apps   = @($app1, $app2)
                }
            }
            Mock Install-App {}
            Mock Install-DownloadApp {}
            Mock Invoke-AppConfigurator {}

            Invoke-ConfigureMachine -Environments @('Work')

            Should -Invoke Import-Manifest -Times 1 -Exactly -ParameterFilter {
                $Environments.Count -eq 1 -and $Environments[0] -eq 'Work'
            }
            Should -Invoke Install-App -Times 2 -Exactly
            Should -Not -Invoke Install-DownloadApp
            Should -Invoke Invoke-AppConfigurator -Times 2 -Exactly
            Should -Invoke Invoke-AppConfigurator -ParameterFilter {
                $App.AppId -eq 'app-1' -and $Configure -eq $true
            }
            Should -Invoke Invoke-AppConfigurator -ParameterFilter {
                $App.AppId -eq 'app-2' -and $Configure -eq $true
            }
        }
    }

    It 'uses Install-DownloadApp for download apps' {
        InModuleScope Configurator {
            $downloadApp = [PSCustomObject]@{
                AppId              = 'download-app'
                AppType            = 'powerShellAppPackage'
                IsDownloadApp      = $true
                Downloader         = 'GitHubAssetDownloader'
                DownloaderArgs     = [PSCustomObject]@{ User = 'u'; Repo = 'r'; Extension = '.exe' }
                DownloadedFilePath = $null
                InstallScript      = 'install'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
                Configuration      = $null
            }
            $normalApp = [PSCustomObject]@{
                AppId              = 'normal-app'
                AppType            = 'winget'
                IsDownloadApp      = $false
                InstallScript      = 'install'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
                Configuration      = $null
            }

            Mock Import-Manifest {
                @{
                    AppIds = @('download-app', 'normal-app')
                    Apps   = @($downloadApp, $normalApp)
                }
            }
            Mock Install-App {}
            Mock Install-DownloadApp {}
            Mock Invoke-AppConfigurator {}

            Invoke-ConfigureMachine

            Should -Invoke Install-DownloadApp -Times 1 -Exactly -ParameterFilter {
                $App.AppId -eq 'download-app'
            }
            Should -Invoke Install-App -Times 1 -Exactly -ParameterFilter {
                $App.AppId -eq 'normal-app'
            }
        }
    }

    It 'loads single app by ID when SingleAppId is provided' {
        InModuleScope Configurator {
            $app = [PSCustomObject]@{
                AppId              = 'single-app'
                AppType            = 'winget'
                IsDownloadApp      = $false
                InstallScript      = 'install'
                VerificationScript = $null
                UpgradeScript      = $null
                PreventUpgrade     = $false
                Configuration      = $null
            }

            Mock Import-Manifest {
                @{
                    AppIds = @('single-app')
                    Apps   = @($app)
                }
            }
            Mock Install-App {}
            Mock Install-DownloadApp {}
            Mock Invoke-AppConfigurator {}

            Invoke-ConfigureMachine -SingleAppId 'single-app'

            Should -Invoke Import-Manifest -Times 1 -Exactly -ParameterFilter {
                $SingleAppId -eq 'single-app'
            }
            Should -Invoke Install-App -Times 1 -Exactly
            Should -Invoke Invoke-AppConfigurator -Times 1 -Exactly
        }
    }

    It 'handles empty manifest' {
        InModuleScope Configurator {
            Mock Import-Manifest {
                @{
                    AppIds = @()
                    Apps   = @()
                }
            }
            Mock Install-App {}
            Mock Install-DownloadApp {}
            Mock Invoke-AppConfigurator {}

            Invoke-ConfigureMachine

            Should -Not -Invoke Install-App
            Should -Not -Invoke Install-DownloadApp
            Should -Not -Invoke Invoke-AppConfigurator
        }
    }

    It 'passes multiple environments to Import-Manifest' {
        InModuleScope Configurator {
            Mock Import-Manifest {
                @{
                    AppIds = @()
                    Apps   = @()
                }
            }
            Mock Install-App {}
            Mock Install-DownloadApp {}
            Mock Invoke-AppConfigurator {}

            Invoke-ConfigureMachine -Environments @('Work', 'Personal')

            Should -Invoke Import-Manifest -Times 1 -Exactly -ParameterFilter {
                $Environments.Count -eq 2
            }
        }
    }

    It 'defaults to empty environments when none specified' {
        InModuleScope Configurator {
            Mock Import-Manifest {
                @{
                    AppIds = @()
                    Apps   = @()
                }
            }
            Mock Install-App {}
            Mock Install-DownloadApp {}
            Mock Invoke-AppConfigurator {}

            Invoke-ConfigureMachine

            Should -Invoke Import-Manifest -Times 1 -Exactly -ParameterFilter {
                $Environments.Count -eq 0
            }
        }
    }
}
