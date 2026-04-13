BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force

    $script:testManifestsDir = Join-Path $PSScriptRoot 'TestManifests'
}

Describe 'Save-AppDefinition Integration' {
    It 'saves an installable to a new manifest' {
        InModuleScope Configurator {
            $tempDir = Join-Path $TestDrive 'save-new'
            New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $tempDir
                        fileName  = 'manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            $appId = 'test-save-new-app'
            Save-AppDefinition -AppId $appId -AppType 'winget' -Environments 'Test'

            $manifest = Import-Manifest

            @($manifest.AppIds).Count | Should -Be 1
            $manifest.AppIds[0] | Should -Be $appId
            @($manifest.Apps).Count | Should -Be 1
            $manifest.Apps[0].AppId | Should -Be $appId
            $manifest.Apps[0].AppType | Should -Be 'winget'
        }
    }

    It 'saves an installable to an existing manifest' {
        InModuleScope Configurator {
            $tempDir = Join-Path $TestDrive 'save-existing'
            New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $tempDir
                        fileName  = 'manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            $appId1 = 'test-existing-app-1'
            Save-AppDefinition -AppId $appId1 -AppType 'winget' -Environments 'Test'

            $appId2 = 'test-existing-app-2'
            Save-AppDefinition -AppId $appId2 -AppType 'script' -Environments 'Test'

            $manifest = Import-Manifest

            @($manifest.AppIds).Count | Should -Be 2
            $manifest.AppIds[0] | Should -Be $appId1
            $manifest.AppIds[1] | Should -Be $appId2
            $manifest.Apps[0].AppId | Should -Be $appId1
            $manifest.Apps[0].AppType | Should -Be 'winget'
            $manifest.Apps[1].AppId | Should -Be $appId2
            $manifest.Apps[1].AppType | Should -Be 'script'
        }
    }

    It 'does not duplicate when saving the same app twice' {
        InModuleScope Configurator {
            $tempDir = Join-Path $TestDrive 'save-idempotent'
            New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $tempDir
                        fileName  = 'manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            $appId = 'test-idempotent-app'
            Save-AppDefinition -AppId $appId -AppType 'winget' -Environments 'Test'
            Save-AppDefinition -AppId $appId -AppType 'script' -Environments 'Other'

            $manifest = Import-Manifest

            @($manifest.AppIds).Count | Should -Be 1
            $manifest.AppIds[0] | Should -Be $appId
            @($manifest.Apps).Count | Should -Be 1
            $manifest.Apps[0].AppType | Should -Be 'winget'
        }
    }
}

Describe 'Import-Manifest Integration - Single App' {
    It 'loads a single app by ID' {
        InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'multiple-apps.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            $manifest = Import-Manifest -SingleAppId 'environment-1-and-3-app-id-3'

            @($manifest.Apps).Count | Should -Be 1
            $manifest.Apps[0].AppId | Should -Be 'environment-1-and-3-app-id-3'
        } -ArgumentList $script:testManifestsDir
    }
}

Describe 'Import-Manifest Integration - Winget Apps' {
    BeforeAll {
        $script:wingetManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'winget.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads basic winget app' {
        $app = $script:wingetManifest.Apps[0]
        $app.AppId | Should -Be 'winget-app-id'
        $app.AppType | Should -Be 'winget'
        $app.InstallArgs | Should -BeNullOrEmpty
        $app.PreventUpgrade | Should -BeFalse
        $app.Configuration | Should -BeNullOrEmpty
    }

    It 'loads winget app with install args' {
        $app = $script:wingetManifest.Apps[1]
        $app.AppId | Should -Be 'winget-app-id-with-install-args'
        $app.InstallArgs | Should -Be ' --override install-args'
    }

    It 'loads winget app with prevent upgrade' {
        $app = $script:wingetManifest.Apps[2]
        $app.AppId | Should -Be 'winget-app-id-with-prevent-upgrade'
        $app.PreventUpgrade | Should -BeTrue
    }

    It 'loads winget app with configuration' {
        $app = $script:wingetManifest.Apps[3]
        $app.AppId | Should -Be 'winget-app-id-with-configuration'
        $app.Configuration | Should -Not -BeNullOrEmpty
        $app.Configuration.registrySettings.Count | Should -Be 1
        $app.Configuration.registrySettings[0].keyName | Should -Be 'key-name-test'
        $app.Configuration.registrySettings[0].valueName | Should -Be 'value-name-test'
        $app.Configuration.registrySettings[0].valueData | Should -Be 'value-data-test'
    }
}

Describe 'Import-Manifest Integration - Scoop Apps' {
    BeforeAll {
        $script:scoopManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'scoop.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads basic scoop app' {
        $app = $script:scoopManifest.Apps[0]
        $app.AppId | Should -Be 'scoop-app-id'
        $app.AppType | Should -Be 'scoop'
        $app.InstallArgs | Should -BeNullOrEmpty
        $app.PreventUpgrade | Should -BeFalse
        $app.Configuration | Should -BeNullOrEmpty
    }

    It 'loads scoop app with install args' {
        $app = $script:scoopManifest.Apps[1]
        $app.AppId | Should -Be 'scoop-app-id-with-install-args'
        $app.InstallArgs | Should -Be ' install-args'
    }

    It 'loads scoop app with prevent upgrade' {
        $app = $script:scoopManifest.Apps[2]
        $app.AppId | Should -Be 'scoop-app-id-with-prevent-upgrade'
        $app.PreventUpgrade | Should -BeTrue
    }

    It 'loads scoop app with configuration' {
        $app = $script:scoopManifest.Apps[3]
        $app.AppId | Should -Be 'scoop-app-id-with-configuration'
        $app.Configuration | Should -Not -BeNullOrEmpty
        $app.Configuration.registrySettings.Count | Should -Be 1
        $app.Configuration.registrySettings[0].keyName | Should -Be 'key-name-test'
        $app.Configuration.registrySettings[0].valueName | Should -Be 'value-name-test'
        $app.Configuration.registrySettings[0].valueData | Should -Be 'value-data-test'
    }
}

Describe 'Import-Manifest Integration - Scoop Bucket Apps' {
    It 'loads basic scoop bucket app' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'scoop-bucket.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 1
        $manifest.Apps[0].AppId | Should -Be 'scoop-bucket-app-id'
        $manifest.Apps[0].AppType | Should -Be 'scoopBucket'
    }
}

Describe 'Import-Manifest Integration - Script Apps' {
    BeforeAll {
        $script:scriptManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'script.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads basic script app' {
        $app = $script:scriptManifest.Apps[0]
        $app.AppId | Should -Be 'script-app-id'
        $app.AppType | Should -Be 'script'
        $app.InstallScript | Should -Be 'install-script'
        $app.VerificationScript | Should -Be 'verification-script'
        $app.UpgradeScript | Should -Be 'upgrade-script'
        $app.Configuration | Should -BeNullOrEmpty
    }

    It 'loads script app with configuration' {
        $app = $script:scriptManifest.Apps[1]
        $app.AppId | Should -Be 'script-app-id-with-configuration'
        $app.Configuration | Should -Not -BeNullOrEmpty
        $app.Configuration.registrySettings.Count | Should -Be 1
        $app.Configuration.registrySettings[0].keyName | Should -Be 'key-name-test'
        $app.Configuration.registrySettings[0].valueName | Should -Be 'value-name-test'
        $app.Configuration.registrySettings[0].valueData | Should -Be 'value-data-test'
    }
}

Describe 'Import-Manifest Integration - PowerShell Apps' {
    BeforeAll {
        $script:psManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'powershell.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads basic PowerShell app with all scripts' {
        $app = $script:psManifest.Apps | Where-Object { $_.AppId -eq 'powershell-app-id-1' }
        $app | Should -Not -BeNullOrEmpty
        $app.AppType | Should -Be 'powerShell'

        $expectedInstall = Join-Path $script:testManifestsDir 'apps' 'powershell-app-id-1' 'install.ps1'
        $app.InstallScript | Should -Be ". `"$expectedInstall`""

        $expectedUpgrade = Join-Path $script:testManifestsDir 'apps' 'powershell-app-id-1' 'upgrade.ps1'
        $app.UpgradeScript | Should -Be ". `"$expectedUpgrade`""

        $expectedVerification = Join-Path $script:testManifestsDir 'apps' 'powershell-app-id-1' 'verification.ps1'
        $app.VerificationScript | Should -Be ". `"$expectedVerification`""

        $app.InstallArgs | Should -BeNullOrEmpty
        $app.PreventUpgrade | Should -BeFalse
        $app.Configuration | Should -BeNullOrEmpty
    }

    It 'loads PowerShell app with null non-install scripts when files missing' {
        $app = $script:psManifest.Apps | Where-Object { $_.AppId -eq 'powershell-app-id-2' }
        $app | Should -Not -BeNullOrEmpty
        $app.InstallScript | Should -Not -BeNullOrEmpty
        $app.UpgradeScript | Should -BeNullOrEmpty
        $app.VerificationScript | Should -BeNullOrEmpty
    }

    It 'does not load PowerShell app with missing install script' {
        $app = $script:psManifest.Apps | Where-Object { $_.AppId -eq 'powershell-app-id-3' }
        $app | Should -BeNullOrEmpty
    }
}

Describe 'Import-Manifest Integration - PowerShell Module Apps' {
    BeforeAll {
        $script:psModuleManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'power-shell-module.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads basic PowerShell module app' {
        $app = $script:psModuleManifest.Apps[0]
        $app.AppId | Should -Be 'power-shell-module-app-id'
        $app.AppType | Should -Be 'powerShellModule'
        $app.InstallArgs | Should -BeNullOrEmpty
        $app.PreventUpgrade | Should -BeFalse
        $app.Configuration | Should -BeNullOrEmpty
    }

    It 'loads PowerShell module app with install args' {
        $app = $script:psModuleManifest.Apps[1]
        $app.AppId | Should -Be 'power-shell-module-app-id-with-install-args'
        $app.InstallArgs | Should -Be ' install-args'
    }

    It 'loads PowerShell module app with prevent upgrade' {
        $app = $script:psModuleManifest.Apps[2]
        $app.AppId | Should -Be 'power-shell-module-app-id-with-prevent-upgrade'
        $app.PreventUpgrade | Should -BeTrue
    }

    It 'loads PowerShell module app with configuration' {
        $app = $script:psModuleManifest.Apps[3]
        $app.AppId | Should -Be 'power-shell-module-app-id-with-configuration'
        $app.Configuration | Should -Not -BeNullOrEmpty
        $app.Configuration.registrySettings.Count | Should -Be 1
        $app.Configuration.registrySettings[0].keyName | Should -Be 'key-name-test'
        $app.Configuration.registrySettings[0].valueName | Should -Be 'value-name-test'
        $app.Configuration.registrySettings[0].valueData | Should -Be 'value-data-test'
    }
}

Describe 'Import-Manifest Integration - PowerShell App Package Apps' {
    BeforeAll {
        $script:psAppPkgManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'power-shell-app-packages.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads basic PowerShell app package' {
        $app = $script:psAppPkgManifest.Apps[0]
        $app.AppId | Should -Be 'power-shell-app-package-app-id'
        $app.AppType | Should -Be 'powerShellAppPackage'
        $app.Downloader | Should -Be 'some-downloader'
        $app.DownloaderArgs | Should -Not -BeNullOrEmpty
        $app.PreventUpgrade | Should -BeFalse
        $app.IsDownloadApp | Should -BeTrue
    }

    It 'loads PowerShell app package with prevent upgrade' {
        $app = $script:psAppPkgManifest.Apps[1]
        $app.AppId | Should -Be 'power-shell-app-package-app-id-with-prevent-upgrade'
        $app.Downloader | Should -Be 'some-downloader'
        $app.DownloaderArgs | Should -Not -BeNullOrEmpty
        $app.PreventUpgrade | Should -BeTrue
    }
}

Describe 'Import-Manifest Integration - Gitconfig Apps' {
    It 'loads basic gitconfig app' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'gitconfig.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 1
        $manifest.Apps[0].AppId | Should -Be 'gitconfig-app-id'
        $manifest.Apps[0].AppType | Should -Be 'gitconfig'
    }
}

Describe 'Import-Manifest Integration - Git Repo Apps' {
    BeforeAll {
        $script:gitRepoManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'git-repo.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads basic git repo app and excludes misconfigured ones' {
        @($script:gitRepoManifest.Apps).Count | Should -Be 1
        $app = $script:gitRepoManifest.Apps[0]
        $app.AppId | Should -Be 'git-repo-app-id'
        $app.AppType | Should -Be 'gitRepo'
        $app.InstallArgs | Should -Be 'https://organization/repo.git'
        $app.PreventUpgrade | Should -BeTrue
        $app.InstallScript | Should -BeLike '*C:\src\*'
        $app.InstallScript | Should -BeLike '*https://organization/repo.git*'
        $app.UpgradeScript | Should -BeLike '*C:\src\repo*'
        $app.VerificationScript | Should -BeLike '*C:\src\repo*'
    }
}

Describe 'Import-Manifest Integration - Non-Package Apps' {
    It 'loads basic non-package app' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'non-package.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 1
        $manifest.Apps[0].AppId | Should -Be 'non-package-app-id'
        $manifest.Apps[0].AppType | Should -Be 'nonPackageApp'
    }
}

Describe 'Import-Manifest Integration - Visual Studio Extension Apps' {
    It 'loads basic visual studio extension app' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'visual-studio-extension.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 1
        $app = $manifest.Apps[0]
        $app.AppId | Should -Be 'visual-studio-extension-app-id'
        $app.AppType | Should -Be 'visualStudioExtension'
        $app.IsDownloadApp | Should -BeTrue
        $app.Downloader | Should -Be 'VisualStudioMarketplaceDownloader'
        $app.DownloaderArgs.publisher | Should -Be 'publisher-1'
        $app.DownloaderArgs.extensionName | Should -Be 'extension-name-1'
    }
}

Describe 'Import-Manifest Integration - Unknown Apps' {
    It 'does not load unknown app types' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'unknown.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 0
    }
}

Describe 'Import-Manifest Integration - Registry Settings' {
    BeforeAll {
        $script:regManifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'registry-settings.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir
    }

    It 'loads string registry setting value data' {
        $regSettings = $script:regManifest.Apps[0].Configuration.registrySettings
        $regSettings[0].keyName | Should -Be 'key-1'
        $regSettings[0].valueName | Should -Be 'string'
        $regSettings[0].valueData | Should -Be 'string-data'
    }

    It 'loads string registry setting with environment token' {
        $regSettings = $script:regManifest.Apps[0].Configuration.registrySettings
        $regSettings[1].keyName | Should -Be 'key-2'
        $regSettings[1].valueName | Should -Be 'string'
        $regSettings[1].valueData | Should -Be '{{env:ProgramFiles}}\string-data'
    }

    It 'loads integer registry setting value data' {
        $regSettings = $script:regManifest.Apps[0].Configuration.registrySettings
        $regSettings[2].keyName | Should -Be 'key-3'
        $regSettings[2].valueName | Should -Be 'uint'
        $regSettings[2].valueData | Should -Be 42
    }
}

Describe 'Import-Manifest Integration - Environment Filtering' {
    It 'loads all apps when no environments specified' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'multiple-apps.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 5
    }

    It 'loads apps for a specific environment, ignoring case' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'multiple-apps.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest -Environments @('environment2')
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 1
    }

    It 'loads apps for multiple environments' {
        $manifest = InModuleScope Configurator {
            param($testManifestsDir)

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $testManifestsDir
                        fileName  = 'multiple-apps.manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Import-Manifest -Environments @('Environment2', 'Environment3')
        } -ArgumentList $script:testManifestsDir

        @($manifest.Apps).Count | Should -Be 3
    }
}

Describe 'Add-ConfiguratorApp Integration' {
    It 'saves and loads a new app through the public command' {
        InModuleScope Configurator {
            $tempDir = Join-Path $TestDrive 'add-app-integration'
            New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

            Mock Import-Settings {
                [PSCustomObject]@{
                    manifest = [PSCustomObject]@{
                        directory = $tempDir
                        fileName  = 'manifest.json'
                    }
                    git = [PSCustomObject]@{
                        cloneDirectory = 'C:\src\'
                    }
                }
            }

            Add-ConfiguratorApp -AppId 'integration-test-app' -AppType 'winget' -Environments @('Work', 'Personal')

            $manifest = Import-Manifest

            @($manifest.Apps).Count | Should -Be 1
            $manifest.Apps[0].AppId | Should -Be 'integration-test-app'
            $manifest.Apps[0].AppType | Should -Be 'winget'
        }
    }
}
