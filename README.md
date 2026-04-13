# Configurator

A Windows machine configuration tool written in PowerShell. Reads a manifest repo containing app definitions and installs, upgrades, verifies, configures, and backs up each app.

Requires **PowerShell 7+** (PowerShell Core) and must be run as **Administrator**.

## Quick Start

### Bootstrap a Fresh Machine

Run this from an elevated Windows PowerShell prompt to install Configurator, set the manifest repo, initialize the system, and configure all apps:

```powershell
$bootstrapStopwatch = [Diagnostics.Stopwatch]::StartNew()
Set-ExecutionPolicy RemoteSigned -Force

# Download and install the Configurator module
Invoke-Expression (Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/dannydwarren/configurator/main/configurator-pwsh/Install-Configurator.ps1' -UseBasicParsing).Content

# Restart PATH so the module is available
$env:Path = [System.Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path', 'User')
$env:PSModulePath = [System.Environment]::GetEnvironmentVariable('PSModulePath', 'Machine') + ';' + [System.Environment]::GetEnvironmentVariable('PSModulePath', 'User')

Import-Module Configurator

# Configure and run
pwsh -File C:\Configurator\src\Configurator.ps1 settings set manifest.repo "https://github.com/dannydwarren/machine-configs.git"
pwsh -File C:\Configurator\src\Configurator.ps1 initialize
pwsh -File C:\Configurator\src\Configurator.ps1 configure --environments "All"

$bootstrapStopwatch.Stop()
Write-Output "Total duration: $($bootstrapStopwatch.Elapsed)"
```

### Install from GitHub Release

```powershell
# Run as Administrator
irm https://raw.githubusercontent.com/dannydwarren/configurator/main/configurator-pwsh/Install-Configurator.ps1 | iex
```

This downloads the latest release zip, extracts to `C:\Configurator`, and adds it to the machine PATH and PSModulePath.

### Install from Source

```powershell
git clone https://github.com/dannydwarren/configurator.git C:\src\configurator
Import-Module C:\src\configurator\configurator-pwsh\src\Configurator.psd1
```

## CLI Usage

All commands must be run as Administrator.

```
Configurator.ps1 <command> [options]
```

### Commands

#### `initialize`

Runs system initialization prerequisites (execution policies, winget, PowerShell Core, scoop, git) and clones the manifest repo.

```powershell
.\Configurator.ps1 initialize
```

#### `configure-machine` (alias: `configure`)

Installs, upgrades, and configures apps from the manifest.

```powershell
# Configure all apps
.\Configurator.ps1 configure-machine

# Configure apps for specific environments
.\Configurator.ps1 configure --environments "Work|Personal"
.\Configurator.ps1 configure -e "Work|Personal"

# Configure a single app by ID
.\Configurator.ps1 configure --single-app-id "my-app"
.\Configurator.ps1 configure -app "my-app"
```

| Option | Alias | Description |
|--------|-------|-------------|
| `--environments` | `-e` | Pipe-separated list of environments to target |
| `--single-app-id` | `-app` | Install a single app by its ID (ignores environments) |

#### `settings list`

Display all settings as a table.

```powershell
.\Configurator.ps1 settings list
```

#### `settings set`

Update a single setting by its dotted path name.

```powershell
.\Configurator.ps1 settings set manifest.repo "https://github.com/user/machine-configs.git"
.\Configurator.ps1 settings set manifest.filename "manifest.json"
.\Configurator.ps1 settings set git.clonedirectory "C:\src\"
```

| Setting | Type | Default |
|---------|------|---------|
| `downloadsdirectory` | Uri | `C:/tmp/configurator-downloads` |
| `manifest.repo` | Uri | *(none)* |
| `manifest.filename` | String | `manifest.json` |
| `manifest.directory` | String | *(set during initialize)* |
| `git.clonedirectory` | Uri | `C:\src\` |

#### `add-app` (alias: `add`)

Add a new app definition to the manifest.

```powershell
.\Configurator.ps1 add-app --app-id "my-app" --app-type "winget" --environments "Work|Personal"
.\Configurator.ps1 add --app-id "my-app" --app-type "scoop" --environments "All"
```

| Option | Description |
|--------|-------------|
| `--app-id` | The app identifier |
| `--app-type` | The installer type (see supported types below) |
| `--environments` | Pipe-separated environment list |

Supported app types: `winget`, `scoop`, `scoopBucket`, `powerShell`, `powerShellModule`, `powerShellAppPackage`, `script`, `gitRepo`, `gitconfig`, `nonPackageApp`, `visualStudioExtension`

#### `backup`

Run backup scripts for all installed apps that have a `backup.ps1` in their manifest directory.

```powershell
.\Configurator.ps1 backup
```

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | Generic failure (invalid args, command error) |
| 2 | Not running as Administrator |

## Module Functions

When imported as a module, the following functions are available:

| Function | Description |
|----------|-------------|
| `Start-Configurator` | CLI entry point (argument parsing and routing) |
| `Invoke-Initialize` | Run system initialization |
| `Invoke-ConfigureMachine` | Install/configure apps from manifest |
| `Get-ConfiguratorSettings` | List all settings |
| `Set-ConfiguratorSetting` | Update a setting |
| `Add-ConfiguratorApp` | Add an app to the manifest |
| `Invoke-Backup` | Back up all installed apps |

## Manifest Format

The manifest repo contains:

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

### Running Tests

```powershell
cd configurator-pwsh
Invoke-Pester -Path tests/ -Output Detailed
```

### Project Structure

```
configurator-pwsh/
  src/
    Configurator.ps1       # CLI entry point
    Configurator.psd1      # Module manifest
    Configurator.psm1      # Module loader
    Public/                 # Exported command functions
    Private/                # Internal functions
      AppTypes/             # App type parsers (12 types)
      Downloaders/          # GitHub asset, VS extension downloaders
      Initializer/          # System initialization steps
      Installer/            # Install/upgrade/configure/backup logic
      Manifest/             # Manifest loading and saving
      PowerShell/           # Script execution wrappers
      Registry/             # Windows registry operations
      Settings/             # Settings persistence
      Utilities/            # Logging, tokens, desktop cleanup, etc.
  tests/
    Unit/                   # Pester unit tests
    Integration/            # Pester integration tests
  spec/                     # Behavior specification docs
  Install-Configurator.ps1 # Bootstrap installer script
```

## Previous C# Version

This tool was rewritten from C# to PowerShell. The original C# README is archived at [archive/README-csharp.md](archive/README-csharp.md).
