# Configurator

A Windows machine configuration tool. PowerShell 7.0+ module, must be run as Administrator.

```powershell
Set-ExecutionPolicy RemoteSigned -Force

# Download and install the Configurator module
irm https://raw.githubusercontent.com/dannydwarren/configurator/main/configurator-pwsh/Install-Configurator.ps1 | iex

# Restart PATH so the module is available
$env:Path = [System.Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path', 'User')
$env:PSModulePath = [System.Environment]::GetEnvironmentVariable('PSModulePath', 'Machine') + ';' + [System.Environment]::GetEnvironmentVariable('PSModulePath', 'User')

# Configure and run
Configurator.ps1 settings set manifest.repo "https://github.com/dannydwarren/machine-configs.git"
Configurator.ps1 initialize
Configurator.ps1 configure --environments "All"
```

## CLI Commands

```
Configurator.ps1 <command> [options]
```

| Command | Description |
|---------|-------------|
| `initialize` | Run system initialization (execution policies, winget, PS Core, scoop, git) and clone manifest repo |
| `configure-machine` | Install/upgrade/configure apps from manifest. Alias: `configure` |
| `settings list` | Display all settings |
| `settings set <name> <value>` | Update a setting (e.g. `manifest.repo`, `git.clonedirectory`) |
| `add-app` | Add a new app to the manifest. Alias: `add` |
| `backup` | Run backup scripts for all installed apps |

### configure-machine options

| Option | Alias | Description |
|--------|-------|-------------|
| `--environments` | `-e` | Pipe-separated environment list (e.g. `"Work\|Personal"`) |
| `--single-app-id` | `-app` | Install a single app by ID (ignores environments) |

### add-app options

| Option | Description |
|--------|-------------|
| `--app-id` | App identifier |
| `--app-type` | Installer type: `winget`, `scoop`, `scoopBucket`, `powerShell`, `powerShellModule`, `powerShellAppPackage`, `script`, `gitRepo`, `gitconfig`, `nonPackageApp`, `visualStudioExtension` |
| `--environments` | Pipe-separated environment list |

### Exit codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Generic failure |
| 2 | Not running as Administrator |

## Manifest Format

```
manifest.json          # List of app IDs
apps/
  <app-id>/
    app.json           # App definition (type, environments, config)
    install.ps1        # Install script (PowerShell app type)
    upgrade.ps1        # Upgrade script (optional)
    verification.ps1   # Verification script (optional)
    backup.ps1         # Backup script (optional)
```

## Development

```powershell
cd configurator-pwsh
Invoke-Pester -Path tests/ -Output Detailed
```

## Previous C# Version

For the previous C# implementation, see [archive/README-csharp.md](archive/README-csharp.md).
