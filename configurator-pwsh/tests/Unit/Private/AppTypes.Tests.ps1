BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'New-WingetApp' {
    It 'creates app with correct scripts' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'winget'
                appId          = 'Microsoft.VisualStudioCode'
                environments   = 'Work|Personal'
                installArgs    = $null
                preventUpgrade = $false
                configuration  = $null
            }

            $result = New-WingetApp -RawApp $raw

            $result.AppId | Should -Be 'Microsoft.VisualStudioCode'
            $result.AppType | Should -Be 'winget'
            $result.InstallScript | Should -Be 'winget install --id Microsoft.VisualStudioCode --accept-package-agreements -h -e'
            $result.VerificationScript | Should -Be '(winget list --id Microsoft.VisualStudioCode -e | Select-String Microsoft.VisualStudioCode) -ne $null'
            $result.UpgradeScript | Should -Be 'winget upgrade --id Microsoft.VisualStudioCode --accept-package-agreements -h -e'
            $result.PreventUpgrade | Should -BeFalse
            $result.IsDownloadApp | Should -BeFalse
        }
    }

    It 'transforms installArgs with --override prefix' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'winget'
                appId          = 'Some.App'
                environments   = 'Work'
                installArgs    = 'install-args'
                preventUpgrade = $false
                configuration  = $null
            }

            $result = New-WingetApp -RawApp $raw

            $result.InstallArgs | Should -Be ' --override install-args'
            $result.InstallScript | Should -BeLike '* --override install-args'
        }
    }

    It 'preserves preventUpgrade flag' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'winget'
                appId          = 'Some.App'
                environments   = 'Work'
                installArgs    = $null
                preventUpgrade = $true
                configuration  = $null
            }

            $result = New-WingetApp -RawApp $raw

            $result.PreventUpgrade | Should -BeTrue
        }
    }

    It 'preserves configuration' {
        InModuleScope Configurator {
            $config = [PSCustomObject]@{
                registrySettings = @(
                    [PSCustomObject]@{
                        keyName   = 'HKCU\Test'
                        valueName = 'Setting'
                        valueData = 'Value'
                    }
                )
            }

            $raw = [PSCustomObject]@{
                appType        = 'winget'
                appId          = 'Some.App'
                environments   = 'Work'
                installArgs    = $null
                preventUpgrade = $false
                configuration  = $config
            }

            $result = New-WingetApp -RawApp $raw

            $result.Configuration | Should -Not -BeNullOrEmpty
            $result.Configuration.registrySettings.Count | Should -Be 1
        }
    }
}

Describe 'New-ScoopApp' {
    It 'creates app with correct scripts' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'scoop'
                appId          = '7zip'
                environments   = 'Personal'
                installArgs    = $null
                preventUpgrade = $false
                configuration  = $null
            }

            $result = New-ScoopApp -RawApp $raw

            $result.AppId | Should -Be '7zip'
            $result.InstallScript | Should -Be 'scoop install 7zip'
            $result.VerificationScript | Should -Be '(scoop export | Select-String 7zip) -ne $null'
            $result.UpgradeScript | Should -Be 'scoop update 7zip'
        }
    }

    It 'prepends space to installArgs' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'scoop'
                appId          = '7zip'
                environments   = 'Personal'
                installArgs    = 'install-args'
                preventUpgrade = $false
                configuration  = $null
            }

            $result = New-ScoopApp -RawApp $raw

            $result.InstallArgs | Should -Be ' install-args'
            $result.InstallScript | Should -Be 'scoop install 7zip install-args'
        }
    }
}

Describe 'New-ScoopBucketApp' {
    It 'creates app with correct scripts and fixed verification' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType      = 'scoopBucket'
                appId        = 'extras'
                environments = 'Personal'
            }

            $result = New-ScoopBucketApp -RawApp $raw

            $result.AppId | Should -Be 'extras'
            $result.InstallScript | Should -Be 'scoop bucket add extras'
            $result.VerificationScript | Should -BeLike "*(scoop bucket list | Select-String 'extras')*"
            $result.UpgradeScript | Should -BeNullOrEmpty
            $result.PreventUpgrade | Should -BeFalse
            $result.Configuration | Should -BeNullOrEmpty
        }
    }
}

Describe 'New-PowerShellApp' {
    It 'loads scripts from files' {
        InModuleScope Configurator {
            $manifestDir = Join-Path $TestDrive 'ps-manifest'
            $appDir = Join-Path $manifestDir 'apps' 'my-ps-app'
            New-Item -Path $appDir -ItemType Directory -Force | Out-Null

            Set-Content -Path (Join-Path $appDir 'install.ps1') -Value 'Write-Host install'
            Set-Content -Path (Join-Path $appDir 'upgrade.ps1') -Value 'Write-Host upgrade'
            Set-Content -Path (Join-Path $appDir 'verification.ps1') -Value 'Write-Host verify'

            $raw = [PSCustomObject]@{
                appType      = 'powerShell'
                appId        = 'my-ps-app'
                environments = 'Work'
            }

            $result = New-PowerShellApp -RawApp $raw -ManifestDirectory $manifestDir

            $result.AppId | Should -Be 'my-ps-app'
            $result.InstallScript | Should -BeLike '. "*install.ps1"'
            $result.UpgradeScript | Should -BeLike '. "*upgrade.ps1"'
            $result.VerificationScript | Should -BeLike '. "*verification.ps1"'
        }
    }

    It 'returns null when install.ps1 is missing' {
        InModuleScope Configurator {
            $manifestDir = Join-Path $TestDrive 'ps-manifest-missing'
            $appDir = Join-Path $manifestDir 'apps' 'no-install-app'
            New-Item -Path $appDir -ItemType Directory -Force | Out-Null

            $raw = [PSCustomObject]@{
                appType      = 'powerShell'
                appId        = 'no-install-app'
                environments = 'Work'
            }

            $result = New-PowerShellApp -RawApp $raw -ManifestDirectory $manifestDir

            $result | Should -BeNullOrEmpty
        }
    }

    It 'sets upgrade and verification to null when files missing' {
        InModuleScope Configurator {
            $manifestDir = Join-Path $TestDrive 'ps-manifest-partial'
            $appDir = Join-Path $manifestDir 'apps' 'partial-app'
            New-Item -Path $appDir -ItemType Directory -Force | Out-Null
            Set-Content -Path (Join-Path $appDir 'install.ps1') -Value 'Write-Host install'

            $raw = [PSCustomObject]@{
                appType      = 'powerShell'
                appId        = 'partial-app'
                environments = 'Work'
            }

            $result = New-PowerShellApp -RawApp $raw -ManifestDirectory $manifestDir

            $result | Should -Not -BeNullOrEmpty
            $result.UpgradeScript | Should -BeNullOrEmpty
            $result.VerificationScript | Should -BeNullOrEmpty
        }
    }
}

Describe 'New-PowerShellModuleApp' {
    It 'creates app with correct multi-line scripts' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'powerShellModule'
                appId          = 'posh-git'
                environments   = 'Work'
                installArgs    = $null
                preventUpgrade = $false
                configuration  = $null
            }

            $result = New-PowerShellModuleApp -RawApp $raw

            $result.InstallScript | Should -BeLike 'Import-Module PowerShellGet*Install-Module*posh-git'
            $result.VerificationScript | Should -Be '(Get-Module -ListAvailable posh-git) -ne $null'
            $result.UpgradeScript | Should -BeLike 'Import-Module PowerShellGet*Update-Module*posh-git'
        }
    }

    It 'prepends space to installArgs' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'powerShellModule'
                appId          = 'posh-git'
                environments   = 'Work'
                installArgs    = '-Force -AllowClobber'
                preventUpgrade = $false
                configuration  = $null
            }

            $result = New-PowerShellModuleApp -RawApp $raw

            $result.InstallArgs | Should -Be ' -Force -AllowClobber'
        }
    }
}

Describe 'New-PowerShellAppPackageApp' {
    It 'creates download app with correct scripts' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'powerShellAppPackage'
                appId          = 'Microsoft.DesktopAppInstaller'
                environments   = 'Work'
                preventUpgrade = $false
                downloader     = 'GitHubAssetDownloader'
                downloaderArgs = [PSCustomObject]@{
                    User      = 'microsoft'
                    Repo      = 'winget-cli'
                    Extension = '.msixbundle'
                }
            }

            $result = New-PowerShellAppPackageApp -RawApp $raw

            $result.IsDownloadApp | Should -BeTrue
            $result.Downloader | Should -Be 'GitHubAssetDownloader'
            $result.InstallScript | Should -BeLike 'Import-Module appx*Add-AppPackage*'
            $result.VerificationScript | Should -BeLike 'Import-Module appx*Get-AppPackage*Microsoft.DesktopAppInstaller*'
            $result.UpgradeScript | Should -Be $result.InstallScript
        }
    }
}

Describe 'New-ScriptApp' {
    It 'uses inline scripts from JSON fields' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType            = 'script'
                appId              = 'my-script'
                environments       = 'Work'
                installScript      = 'Write-Host install'
                verificationScript = 'Write-Host verify'
                upgradeScript      = 'Write-Host upgrade'
                configuration      = $null
            }

            $result = New-ScriptApp -RawApp $raw

            $result.InstallScript | Should -Be 'Write-Host install'
            $result.VerificationScript | Should -Be 'Write-Host verify'
            $result.UpgradeScript | Should -Be 'Write-Host upgrade'
            $result.PreventUpgrade | Should -BeFalse
        }
    }
}

Describe 'New-GitRepoApp' {
    It 'creates app with git clone/pull scripts' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'gitRepo'
                appId          = 'git.my-project'
                environments   = 'Work'
                installArgs    = 'https://github.com/org/repo.git'
                preventUpgrade = $false
            }

            $result = New-GitRepoApp -RawApp $raw -CloneRootDirectory 'C:\src\'

            $result.InstallScript | Should -BeLike 'mkdir C:\src\*git clone*'
            $result.VerificationScript | Should -Be 'Test-Path C:\src\repo'
            $result.UpgradeScript | Should -BeLike 'pushd C:\src\repo*git pull*'
        }
    }

    It 'returns null when installArgs is null' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'gitRepo'
                appId          = 'git.bad'
                environments   = 'Work'
                installArgs    = $null
                preventUpgrade = $false
            }

            $result = New-GitRepoApp -RawApp $raw -CloneRootDirectory 'C:\src\'

            $result | Should -BeNullOrEmpty
        }
    }

    It 'returns null when installArgs is whitespace' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'gitRepo'
                appId          = 'git.blank'
                environments   = 'Work'
                installArgs    = '   '
                preventUpgrade = $false
            }

            $result = New-GitRepoApp -RawApp $raw -CloneRootDirectory 'C:\src\'

            $result | Should -BeNullOrEmpty
        }
    }

    It 'appends trailing backslash to clone directory' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'gitRepo'
                appId          = 'git.test'
                environments   = 'Work'
                installArgs    = 'https://github.com/org/repo.git'
                preventUpgrade = $false
            }

            $result = New-GitRepoApp -RawApp $raw -CloneRootDirectory 'C:\src'

            $result.VerificationScript | Should -Be 'Test-Path C:\src\repo'
        }
    }
}

Describe 'New-GitconfigApp' {
    It 'creates app with git config commands' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType      = 'gitconfig'
                appId        = 'path/to/.gitconfig-custom'
                environments = 'Work'
            }

            $result = New-GitconfigApp -RawApp $raw

            $result.InstallScript | Should -Be 'git config --global --add include.path path/to/.gitconfig-custom'
            $result.VerificationScript | Should -BeLike '*(git config --get-all --global include.path) -match*'
            $result.UpgradeScript | Should -BeNullOrEmpty
        }
    }

    It 'escapes backslashes in verification regex' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType      = 'gitconfig'
                appId        = 'path\to\.gitconfig'
                environments = 'Work'
            }

            $result = New-GitconfigApp -RawApp $raw

            $result.VerificationScript | Should -BeLike '*path\\to\\.gitconfig*'
        }
    }
}

Describe 'New-NonPackageApp' {
    It 'creates app with convention-based install script' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType      = 'nonPackageApp'
                appId        = 'my-custom-thing'
                environments = 'Work'
            }

            $result = New-NonPackageApp -RawApp $raw

            $result.InstallScript | Should -Be './my-custom-thing_install.ps1'
            $result.VerificationScript | Should -BeNullOrEmpty
            $result.UpgradeScript | Should -BeNullOrEmpty
        }
    }
}

Describe 'New-VisualStudioExtensionApp' {
    It 'creates download app with vsix installer script' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'visualStudioExtension'
                appId          = 'vs-ext-app'
                environments   = 'Work'
                preventUpgrade = $false
                downloaderArgs = [PSCustomObject]@{
                    Publisher     = 'Shanewho'
                    ExtensionName = 'IHateRegions'
                }
            }

            $result = New-VisualStudioExtensionApp -RawApp $raw

            $result.IsDownloadApp | Should -BeTrue
            $result.Downloader | Should -Be 'VisualStudioMarketplaceDownloader'
            $result.InstallScript | Should -BeLike '*vsixInstaller*'
            $result.InstallScript | Should -Not -BeLike '*vsi0xInstaller*'
            $result.VerificationScript | Should -BeNullOrEmpty
            $result.UpgradeScript | Should -Be $result.InstallScript
        }
    }
}

Describe 'New-GitHubAssetApp' {
    It 'creates internal download app with empty install script' {
        InModuleScope Configurator {
            $raw = [PSCustomObject]@{
                appType        = 'gitHubAsset'
                appId          = 'Configurator'
                environments   = ''
                downloaderArgs = [PSCustomObject]@{
                    User      = 'dannydwarren'
                    Repo      = 'configurator'
                    Extension = '.exe'
                }
            }

            $result = New-GitHubAssetApp -RawApp $raw

            $result.InstallScript | Should -Be ''
            $result.VerificationScript | Should -BeNullOrEmpty
            $result.UpgradeScript | Should -BeNullOrEmpty
            $result.PreventUpgrade | Should -BeTrue
            $result.IsDownloadApp | Should -BeTrue
            $result.Downloader | Should -Be 'GitHubAssetDownloader'
        }
    }
}
