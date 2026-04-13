BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Write-ConfiguratorLog' {
    It 'writes formatted log message with correct level label' {
        InModuleScope Configurator {
            Mock Write-Host {}

            Write-ConfiguratorLog -Message 'test message' -Level Info

            Should -Invoke Write-Host -Times 1 -Exactly -ParameterFilter {
                $Object -match '^\[.+\] \[INFO \] test message$' -and $ForegroundColor -eq 'White'
            }
        }
    }

    It 'uses correct color for each level' {
        InModuleScope Configurator {
            $levels = @{
                Debug    = 'Gray'
                Verbose  = 'White'
                Info     = 'White'
                Warn     = 'Yellow'
                Error    = 'Red'
                Progress = 'Blue'
                Result   = 'Green'
            }

            foreach ($level in $levels.Keys) {
                Mock Write-Host {}

                Write-ConfiguratorLog -Message 'test' -Level $level

                Should -Invoke Write-Host -ParameterFilter { $ForegroundColor -eq $levels[$level] }
            }
        }
    }

    It 'uses ISO8601 timestamp format' {
        InModuleScope Configurator {
            Mock Write-Host {}

            Write-ConfiguratorLog -Message 'ts test' -Level Info

            Should -Invoke Write-Host -ParameterFilter {
                $Object -match '^\[\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}'
            }
        }
    }
}

Describe 'Resolve-Token' {
    It 'resolves a single env token' {
        InModuleScope Configurator {
            $programFiles = [System.Environment]::GetEnvironmentVariable('ProgramFiles')
            $result = Resolve-Token -Value '{{env:ProgramFiles}}\path'
            $result | Should -Be "$programFiles\path"
        }
    }

    It 'resolves multiple env tokens' {
        InModuleScope Configurator {
            $programFiles = [System.Environment]::GetEnvironmentVariable('ProgramFiles')
            $userProfile = [System.Environment]::GetEnvironmentVariable('USERPROFILE')
            $result = Resolve-Token -Value '{{env:ProgramFiles}}\app and {{env:USERPROFILE}}\data'
            $result | Should -Be "$programFiles\app and $userProfile\data"
        }
    }

    It 'replaces unknown env var with empty string' {
        InModuleScope Configurator {
            $result = Resolve-Token -Value '{{env:NONEXISTENT_VAR_12345}}\path'
            $result | Should -Be '\path'
        }
    }

    It 'returns value unchanged when no tokens present' {
        InModuleScope Configurator {
            $result = Resolve-Token -Value 'no tokens here'
            $result | Should -Be 'no tokens here'
        }
    }
}

Describe 'Get-DesktopEntries' {
    It 'returns entries from both desktop paths' {
        InModuleScope Configurator {
            $result = Get-DesktopEntries
            $result | Should -Not -BeNullOrEmpty -Because 'desktop directories typically have entries'
        }
    }
}

Describe 'Remove-DesktopShortcuts' {
    It 'deletes new entries that appear after install' {
        InModuleScope Configurator {
            $testDir = Join-Path ([System.IO.Path]::GetTempPath()) "desktop-test-$([guid]::NewGuid().ToString('N'))"
            New-Item -Path $testDir -ItemType Directory -Force | Out-Null
            $newFile = Join-Path $testDir 'new-shortcut.lnk'
            Set-Content -Path $newFile -Value 'test'

            $before = @()
            $after = @($newFile)

            Remove-DesktopShortcuts -BeforeEntries $before -AfterEntries $after

            Test-Path $newFile | Should -BeFalse

            Remove-Item -Path $testDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'does not delete existing entries' {
        InModuleScope Configurator {
            $testDir = Join-Path ([System.IO.Path]::GetTempPath()) "desktop-test-$([guid]::NewGuid().ToString('N'))"
            New-Item -Path $testDir -ItemType Directory -Force | Out-Null
            $existingFile = Join-Path $testDir 'existing.lnk'
            Set-Content -Path $existingFile -Value 'test'

            $before = @($existingFile)
            $after = @($existingFile)

            Remove-DesktopShortcuts -BeforeEntries $before -AfterEntries $after

            Test-Path $existingFile | Should -BeTrue

            Remove-Item -Path $testDir -Recurse -Force
        }
    }
}

Describe 'Test-Administrator' {
    It 'returns a boolean value' {
        InModuleScope Configurator {
            $result = Test-Administrator
            $result | Should -BeOfType [bool]
        }
    }
}

Describe 'Get-Download' {
    It 'throws on non-200 status' {
        InModuleScope Configurator {
            $testDir = Join-Path ([System.IO.Path]::GetTempPath()) "download-test-$([guid]::NewGuid().ToString('N'))"
            New-Item -Path $testDir -ItemType Directory -Force | Out-Null

            Mock Invoke-WebRequest { throw "404 not found" }

            { Get-Download -Url 'https://invalid.example.com/file.exe' -FileName 'file.exe' -DownloadsDirectory $testDir } |
                Should -Throw

            Remove-Item -Path $testDir -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'New-ScriptFile' {
    It 'writes script to temp file and returns path' {
        InModuleScope Configurator {
            $result = New-ScriptFile -Script 'Write-Host "hello"'

            $result | Should -Match '\.ps1$'
            Test-Path $result | Should -BeTrue
            $content = Get-Content -Path $result -Raw
            $content | Should -Match 'Write-Host "hello"'

            Remove-Item -Path $result -Force
        }
    }

    It 'creates file in Configurator temp directory' {
        InModuleScope Configurator {
            $result = New-ScriptFile -Script 'test'
            $expectedDir = Join-Path $env:LOCALAPPDATA 'Configurator' 'temp'
            $result | Should -BeLike "$expectedDir*"

            Remove-Item -Path $result -Force
        }
    }

    It 'uses timestamp format in filename' {
        InModuleScope Configurator {
            $result = New-ScriptFile -Script 'test'
            $fileName = [System.IO.Path]::GetFileNameWithoutExtension($result)
            $fileName | Should -Match '^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}-\d{5}$'

            Remove-Item -Path $result -Force
        }
    }
}

Describe 'Find-PowerShellCore' {
    It 'returns a path to pwsh.exe when PowerShell Core is installed' {
        InModuleScope Configurator {
            $regKey = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey('SOFTWARE\Microsoft\PowerShellCore\InstalledVersions')
            if ($null -eq $regKey) {
                Set-ItResult -Skipped -Because 'PowerShell Core registry key not found'
                return
            }
            $regKey.Close()

            $result = Find-PowerShellCore
            $result | Should -Match 'pwsh\.exe$'
        }
    }
}
