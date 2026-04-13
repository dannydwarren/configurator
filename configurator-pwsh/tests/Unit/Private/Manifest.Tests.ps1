BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Import-AppDefinition' {
    It 'loads app.json and returns raw PSCustomObject' {
        $testDir = Join-Path $TestDrive 'manifest'
        $appDir = Join-Path $testDir 'apps' 'test-app'
        New-Item -Path $appDir -ItemType Directory -Force | Out-Null

        $appJson = @{
            appType      = 'winget'
            appId        = 'test-app'
            environments = 'Work'
            installArgs  = 'some-args'
        } | ConvertTo-Json
        Set-Content -Path (Join-Path $appDir 'app.json') -Value $appJson

        $result = Import-AppDefinition -AppId 'test-app' -ManifestDirectory $testDir

        $result.appType | Should -Be 'winget'
        $result.appId | Should -Be 'test-app'
        $result.environments | Should -Be 'Work'
        $result.installArgs | Should -Be 'some-args'
    }
}

Describe 'ConvertTo-AppObject' {
    BeforeAll {
        $settings = [PSCustomObject]@{
            manifest = [PSCustomObject]@{
                directory = $TestDrive
                fileName  = 'manifest.json'
            }
            git = [PSCustomObject]@{
                cloneDirectory = 'C:\src\'
            }
        }
    }

    It 'routes winget app type correctly' {
        $raw = [PSCustomObject]@{
            appType      = 'winget'
            appId        = 'Some.App'
            environments = 'Work'
            installArgs  = $null
            preventUpgrade = $false
            configuration = $null
        }

        $result = ConvertTo-AppObject -RawApp $raw -Settings $settings

        $result.AppType | Should -Be 'winget'
        $result.AppId | Should -Be 'Some.App'
        $result.InstallScript | Should -BeLike 'winget install*Some.App*'
    }

    It 'routes scoop app type correctly' {
        $raw = [PSCustomObject]@{
            appType      = 'scoop'
            appId        = '7zip'
            environments = 'Work'
            installArgs  = $null
            preventUpgrade = $false
            configuration = $null
        }

        $result = ConvertTo-AppObject -RawApp $raw -Settings $settings

        $result.AppType | Should -Be 'scoop'
        $result.InstallScript | Should -BeLike 'scoop install*7zip*'
    }

    It 'returns null for unknown app type' {
        $raw = [PSCustomObject]@{
            appType      = 'unknown'
            appId        = 'bad-app'
            environments = 'Work'
        }

        $result = ConvertTo-AppObject -RawApp $raw -Settings $settings

        $result | Should -BeNullOrEmpty
    }
}

Describe 'Import-Manifest' {
    BeforeAll {
        $manifestDir = Join-Path $TestDrive 'manifest-load'
        New-Item -Path $manifestDir -ItemType Directory -Force | Out-Null

        $app1Dir = Join-Path $manifestDir 'apps' 'winget-app'
        New-Item -Path $app1Dir -ItemType Directory -Force | Out-Null
        @{ appType = 'winget'; appId = 'winget-app'; environments = 'Work' } |
            ConvertTo-Json | Set-Content -Path (Join-Path $app1Dir 'app.json')

        $app2Dir = Join-Path $manifestDir 'apps' 'scoop-app'
        New-Item -Path $app2Dir -ItemType Directory -Force | Out-Null
        @{ appType = 'scoop'; appId = 'scoop-app'; environments = 'Personal' } |
            ConvertTo-Json | Set-Content -Path (Join-Path $app2Dir 'app.json')

        @{ apps = @('winget-app', 'scoop-app') } |
            ConvertTo-Json | Set-Content -Path (Join-Path $manifestDir 'manifest.json')
    }

    It 'loads all apps when no environments specified' {
        Mock Import-Settings {
            [PSCustomObject]@{
                manifest = [PSCustomObject]@{
                    directory = $manifestDir
                    fileName  = 'manifest.json'
                }
                git = [PSCustomObject]@{
                    cloneDirectory = 'C:\src\'
                }
            }
        }

        $result = Import-Manifest

        $result.AppIds.Count | Should -Be 2
        $result.Apps.Count | Should -Be 2
    }

    It 'filters apps by environment' {
        Mock Import-Settings {
            [PSCustomObject]@{
                manifest = [PSCustomObject]@{
                    directory = $manifestDir
                    fileName  = 'manifest.json'
                }
                git = [PSCustomObject]@{
                    cloneDirectory = 'C:\src\'
                }
            }
        }

        $result = Import-Manifest -Environments @('Work')

        $result.Apps.Count | Should -Be 1
        $result.Apps[0].AppId | Should -Be 'winget-app'
    }

    It 'loads single app by ID' {
        Mock Import-Settings {
            [PSCustomObject]@{
                manifest = [PSCustomObject]@{
                    directory = $manifestDir
                    fileName  = 'manifest.json'
                }
                git = [PSCustomObject]@{
                    cloneDirectory = 'C:\src\'
                }
            }
        }

        $result = Import-Manifest -SingleAppId 'scoop-app'

        $result.Apps.Count | Should -Be 1
        $result.Apps[0].AppId | Should -Be 'scoop-app'
    }
}

Describe 'Save-AppDefinition' {
    It 'creates app directory and writes app.json' {
        $manifestDir = Join-Path $TestDrive 'save-test'
        New-Item -Path $manifestDir -ItemType Directory -Force | Out-Null
        @{ apps = @() } | ConvertTo-Json | Set-Content -Path (Join-Path $manifestDir 'manifest.json')

        Mock Import-Settings {
            [PSCustomObject]@{
                manifest = [PSCustomObject]@{
                    directory = $manifestDir
                    fileName  = 'manifest.json'
                }
                git = [PSCustomObject]@{
                    cloneDirectory = 'C:\src\'
                }
            }
        }

        Save-AppDefinition -AppId 'new-app' -AppType 'winget' -Environments 'Work|Personal'

        $appFile = Join-Path $manifestDir 'apps' 'new-app' 'app.json'
        Test-Path $appFile | Should -BeTrue

        $appData = Get-Content $appFile -Raw | ConvertFrom-Json
        $appData.appType | Should -Be 'winget'
        $appData.appId | Should -Be 'new-app'
        $appData.environments | Should -Be 'Work|Personal'

        $manifest = Get-Content (Join-Path $manifestDir 'manifest.json') -Raw | ConvertFrom-Json
        $manifest.apps | Should -Contain 'new-app'
    }

    It 'is idempotent - does not duplicate app' {
        $manifestDir = Join-Path $TestDrive 'save-idempotent'
        New-Item -Path $manifestDir -ItemType Directory -Force | Out-Null
        @{ apps = @('existing-app') } | ConvertTo-Json | Set-Content -Path (Join-Path $manifestDir 'manifest.json')

        $appDir = Join-Path $manifestDir 'apps' 'existing-app'
        New-Item -Path $appDir -ItemType Directory -Force | Out-Null
        @{ appType = 'winget'; appId = 'existing-app'; environments = 'Work' } |
            ConvertTo-Json | Set-Content -Path (Join-Path $appDir 'app.json')

        Mock Import-Settings {
            [PSCustomObject]@{
                manifest = [PSCustomObject]@{
                    directory = $manifestDir
                    fileName  = 'manifest.json'
                }
                git = [PSCustomObject]@{
                    cloneDirectory = 'C:\src\'
                }
            }
        }

        Save-AppDefinition -AppId 'existing-app' -AppType 'winget' -Environments 'Work'

        $manifest = Get-Content (Join-Path $manifestDir 'manifest.json') -Raw | ConvertFrom-Json
        @($manifest.apps).Count | Should -Be 1
    }
}
