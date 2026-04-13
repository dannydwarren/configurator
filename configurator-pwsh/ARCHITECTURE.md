# Configurator PowerShell Module - Architecture

## Overview

Configurator is a Windows machine configuration tool rewritten from C# to pure PowerShell. It reads a manifest repo containing app definitions and installs, upgrades, verifies, and configures each app. The PowerShell version consumes the exact same manifest format as the C# version.

## Module Structure

This is a PowerShell script module (not binary).

- **Entry point:** `Configurator.ps1` - main CLI script that users invoke directly
- **Module:** `Configurator.psm1` + `Configurator.psd1` - importable functions for the module system
- **Public functions:** Exported CLI commands (one function per command)
- **Private functions:** Internal utilities not exported from the module

## Directory Layout

```
configurator-pwsh/
  spec/                              # Behavior specification docs
    SPECIFICATION.md
    APP_TYPES.md
    TEST_PLAN.md
  src/
    Configurator.ps1                 # CLI entry point (argument parsing, routing)
    Configurator.psd1                # Module manifest
    Configurator.psm1                # Root module (dot-sources all functions)
    Public/                          # Exported functions (CLI commands)
      Invoke-Initialize.ps1
      Invoke-ConfigureMachine.ps1
      Get-ConfiguratorSettings.ps1
      Set-ConfiguratorSetting.ps1
      Add-ConfiguratorApp.ps1
      Invoke-Backup.ps1
    Private/                         # Internal functions
      Settings/
        Get-SettingsPath.ps1
        Import-Settings.ps1
        Export-Settings.ps1
      Manifest/
        Import-Manifest.ps1
        Import-AppDefinition.ps1
        Save-AppDefinition.ps1
        ConvertTo-AppObject.ps1
      Installer/
        Install-App.ps1
        Install-DownloadApp.ps1
        Invoke-AppConfigurator.ps1
      Initializer/
        Initialize-System.ps1
        Install-Self.ps1
        Install-ScoopCli.ps1
        Install-Git.ps1
        Install-PowerShellCore.ps1
        Install-ManifestRepo.ps1
        Set-WingetConfiguration.ps1
        Set-PowerShellPolicy.ps1
      PowerShell/
        Invoke-PowerShellScript.ps1
        Invoke-WindowsPowerShellScript.ps1
        New-ScriptFile.ps1
        Find-PowerShellCore.ps1
      Utilities/
        Write-ConfiguratorLog.ps1
        Resolve-Token.ps1
        Remove-DesktopShortcuts.ps1
        Get-Download.ps1
        Test-Administrator.ps1
      Downloaders/
        Get-GitHubAsset.ps1
        Get-VisualStudioExtension.ps1
      Registry/
        Get-RegistryValue.ps1
        Set-RegistryValue.ps1
      AppTypes/
        New-WingetApp.ps1
        New-ScoopApp.ps1
        New-ScoopBucketApp.ps1
        New-PowerShellApp.ps1
        New-PowerShellModuleApp.ps1
        New-PowerShellAppPackageApp.ps1
        New-ScriptApp.ps1
        New-GitRepoApp.ps1
        New-GitconfigApp.ps1
        New-NonPackageApp.ps1
        New-VisualStudioExtensionApp.ps1
        New-GitHubAssetApp.ps1
  tests/
    Unit/                            # Pester unit tests (mirror src structure)
      Public/
      Private/
    Integration/                     # Pester integration tests
      TestManifests/                 # Port from C# IntegrationTests
    Configurator.Tests.ps1           # Pester configuration / test entry
  .github/
    workflows/
      ConfiguratorPwshCI.yml         # CI: run Pester tests
      ConfiguratorPwshCICD.yml       # CD: package and release
```

## Design Decisions

### 1. CLI Argument Parsing

Uses `param()` blocks with PowerShell parameter sets instead of System.CommandLine. The main `Configurator.ps1` script handles argument parsing and routes to the appropriate Public function. Each command is a separate parameter set. Aliases (`configure` for `configure-machine`, `add` for `add-app`) are implemented via parameter set names or ValidateSet attributes.

### 2. No DI Container

PowerShell does not need dependency injection. Functions call other functions directly. For testability, Pester's `Mock` command intercepts function calls at the function level. This gives the same isolation as constructor injection without the ceremony.

### 3. App Type Pattern

Each app type has a `New-<Type>App` function in `Private/AppTypes/`. These functions accept the raw JSON-parsed hashtable from `app.json` and return a standardized `[PSCustomObject]` with these properties:

- `AppId`, `AppType`, `Environments`
- `InstallScript`, `VerificationScript`, `UpgradeScript`
- `PreventUpgrade`, `InstallArgs`, `Configuration`
- `Downloader`, `DownloaderArgs`, `DownloadedFilePath` (for download app types)

The `ConvertTo-AppObject` function acts as the discriminator/switch, routing to the correct `New-*App` function based on `appType`.

### 4. Settings as PSCustomObject

Settings use the same JSON format as the C# version. `ConvertFrom-Json` deserializes to `[PSCustomObject]`. `ConvertTo-Json` serializes back. Default values are constructed in `Import-Settings` when no settings file exists.

Settings path: `$env:LOCALAPPDATA\Configurator\settings.json`

JSON property naming uses camelCase to maintain backward compatibility with existing settings files.

### 5. Logging

Custom `Write-ConfiguratorLog` function with level/color mapping matching the C# spec:

| Level    | Label   | Color   |
|----------|---------|---------|
| Debug    | DEBUG   | Gray    |
| Verbose  | INFOV   | White   |
| Info     | INFO    | White   |
| Warn     | WARN    | Yellow  |
| Error    | ERROR   | Red     |
| Progress | PRGRS   | Blue    |
| Result   | RESLT   | Green   |

Format: `[<ISO8601>] [<LEVEL>] <message>`

### 6. Error Handling

`$ErrorActionPreference = 'Stop'` at the module level to ensure errors throw exceptions. Commands use `try/catch` blocks. Exit codes match the spec:

| Code | Name                 |
|------|----------------------|
| 0    | Success              |
| 1    | GenericFailure       |
| 2    | NotEnoughPrivileges  |

### 7. PowerShell Execution

Since we ARE PowerShell, most script execution uses `Invoke-Command` with script blocks or `& operator` instead of spawning pwsh.exe processes. Scripts are still written to temp files and executed via `-File` for isolation and consistent behavior with the C# version's approach.

Exception: `ForceWindowsPowerShell` scenarios still spawn `powershell.exe` explicitly. This is used during initialization when installing PowerShell Core via Windows PowerShell.

The environment-ready script wrapper (PATH rebuild + `$profile` setup) is prepended to every script before execution, matching the C# behavior.

### 8. Self-Install

The PowerShell version downloads a zip/folder from GitHub releases containing the module, extracts to the install directory, and adds to PATH and PSModulePath. This replaces the C# .exe download mechanism.

### 9. Manifest Backward Compatibility

The module reads the exact same `manifest.json` and `apps/<appId>/app.json` format. JSON property names are camelCase. Environment filtering uses case-insensitive substring matching (matching the C# `Contains` behavior).

### 10. Testability

Every function is individually mockable via Pester. Functions accept parameters rather than reading global state directly (except settings, which are loaded via `Import-Settings`). Integration tests use real file I/O with test manifest directories copied from the C# test suite.

### 11. Bug Fixes from C# Version

The following C# bugs are fixed in this rewrite:
- `ScoopBucketApp` verification script now properly interpolates the app ID
- `VisualStudioExtensionApp` install script fixes `$vsi0xInstaller` typo to `$vsixInstaller`
- `GitHubAssestApp` typo corrected to `GitHubAssetApp`
