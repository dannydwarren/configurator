BeforeAll {
    $modulePath = Join-Path $PSScriptRoot '..' '..' '..' 'src' 'Configurator.psd1'
    Import-Module $modulePath -Force
}

Describe 'Add-ConfiguratorApp' {
    BeforeEach {
        Mock Save-AppDefinition {}
    }

    It 'joins environments with pipe and calls Save-AppDefinition for winget app' {
        Add-ConfiguratorApp -AppId 'Some.WingetApp' -AppType 'winget' -Environments @('Work', 'Personal')

        Should -Invoke Save-AppDefinition -Times 1 -ParameterFilter {
            $AppId -eq 'Some.WingetApp' -and
            $AppType -eq 'winget' -and
            $Environments -eq 'Work|Personal'
        }
    }

    It 'joins environments with pipe and calls Save-AppDefinition for scoop app' {
        Add-ConfiguratorApp -AppId 'some-scoop-app' -AppType 'scoop' -Environments @('Dev', 'Test')

        Should -Invoke Save-AppDefinition -Times 1 -ParameterFilter {
            $AppId -eq 'some-scoop-app' -and
            $AppType -eq 'scoop' -and
            $Environments -eq 'Dev|Test'
        }
    }

    It 'handles single environment' {
        Add-ConfiguratorApp -AppId 'single-env-app' -AppType 'gitconfig' -Environments @('Work')

        Should -Invoke Save-AppDefinition -Times 1 -ParameterFilter {
            $Environments -eq 'Work'
        }
    }

    It 'rejects invalid app types' {
        { Add-ConfiguratorApp -AppId 'bad' -AppType 'invalidType' -Environments @('Work') } |
            Should -Throw
    }
}
