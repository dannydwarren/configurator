function Start-Configurator {
    [CmdletBinding()]
    param(
        [Parameter()]
        [string]$Command,

        [Parameter()]
        [string[]]$RemainingArgs
    )

    if (-not (Test-Administrator)) {
        Write-ConfiguratorLog -Message 'Configurator Cli must be run with elevated privileges.' -Level Error
        return 2
    }

    if ([string]::IsNullOrWhiteSpace($Command)) {
        Show-ConfiguratorHelp
        return 0
    }

    try {
        switch ($Command.ToLower()) {
            'initialize' {
                Invoke-Initialize
                return 0
            }

            { $_ -in 'configure-machine', 'configure' } {
                $envString = Get-NamedArgValue -ArgList $RemainingArgs -Names @('--environments', '-e')
                $singleApp = Get-NamedArgValue -ArgList $RemainingArgs -Names @('--single-app-id', '-app')

                $envList = @()
                if (-not [string]::IsNullOrWhiteSpace($envString)) {
                    $envList = $envString.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries)
                }

                $params = @{
                    Environments = $envList
                }
                if (-not [string]::IsNullOrWhiteSpace($singleApp)) {
                    $params['SingleAppId'] = $singleApp
                }

                Invoke-ConfigureMachine @params
                return 0
            }

            'settings' {
                if ($null -eq $RemainingArgs -or $RemainingArgs.Count -eq 0) {
                    Show-ConfiguratorHelp
                    return 1
                }

                $subCommand = $RemainingArgs[0].ToLower()

                switch ($subCommand) {
                    'list' {
                        Get-ConfiguratorSettings
                        return 0
                    }
                    'set' {
                        if ($RemainingArgs.Count -lt 3) {
                            Write-ConfiguratorLog -Message 'Usage: settings set <setting-name> <setting-value>' -Level Error
                            return 1
                        }
                        Set-ConfiguratorSetting -Name $RemainingArgs[1] -Value $RemainingArgs[2]
                        return 0
                    }
                    default {
                        Write-ConfiguratorLog -Message "Unknown settings subcommand: $subCommand" -Level Error
                        return 1
                    }
                }
            }

            { $_ -in 'add-app', 'add' } {
                $appId = Get-NamedArgValue -ArgList $RemainingArgs -Names @('--app-id')
                $appType = Get-NamedArgValue -ArgList $RemainingArgs -Names @('--app-type')
                $envString = Get-NamedArgValue -ArgList $RemainingArgs -Names @('--environments')

                if ([string]::IsNullOrWhiteSpace($appId) -or [string]::IsNullOrWhiteSpace($appType) -or [string]::IsNullOrWhiteSpace($envString)) {
                    Write-ConfiguratorLog -Message 'Usage: add-app --app-id <id> --app-type <type> --environments <envs>' -Level Error
                    return 1
                }

                $envList = $envString.Split('|', [System.StringSplitOptions]::RemoveEmptyEntries)

                Add-ConfiguratorApp -AppId $appId -AppType $appType -Environments $envList
                return 0
            }

            'backup' {
                Invoke-Backup
                return 0
            }

            default {
                Write-ConfiguratorLog -Message "Unknown command: $Command" -Level Error
                return 1
            }
        }
    }
    catch {
        Write-ConfiguratorLog -Message $_.Exception.Message -Level Error
        return 1
    }
}

function Show-ConfiguratorHelp {
    [CmdletBinding()]
    param()

    $helpText = @"
Configurator

Usage:
  Configurator.ps1 <command> [options]

Commands:
  initialize                     Runs system initialization and clones the manifest repo in settings.
  configure-machine, configure   Runs all apps of the manifest repo in settings.
  settings list                  List all settings with values.
  settings set <name> <value>    Set single named setting.
  add-app, add                   Add app for use on the next machine.
  backup                         Backup app configurations etc. for use on the next machine.

Options for configure-machine:
  --environments, -e <envs>      Pipe-separated list of environments to target.
  --single-app-id, -app <id>     The single app to install by Id.

Options for add-app:
  --app-id <id>                  Id of the app.
  --app-type <type>              Specifies the installer to use.
  --environments <envs>          Pipe-separated environment list.
"@
    Write-Host $helpText
}

function Get-NamedArgValue {
    [CmdletBinding()]
    param(
        [Parameter()]
        [AllowNull()]
        [string[]]$ArgList,

        [Parameter(Mandatory)]
        [string[]]$Names
    )

    if ($null -eq $ArgList) { return $null }

    for ($i = 0; $i -lt $ArgList.Count; $i++) {
        if ($Names -contains $ArgList[$i] -and ($i + 1) -lt $ArgList.Count) {
            return $ArgList[$i + 1]
        }
    }
    return $null
}
