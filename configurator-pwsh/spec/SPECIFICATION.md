# Configurator PowerShell Rewrite - Behavior Specification

This document is the authoritative behavioral contract for the PowerShell rewrite of the Configurator tool. It was derived by reading every C# source file in the original codebase. Implementers should be able to build from this spec without referencing the C# code.

---

## Table of Contents

1. [Overview](#overview)
2. [CLI Commands](#cli-commands)
3. [Settings Management](#settings-management)
4. [Manifest Loading](#manifest-loading)
5. [App Installation Flow](#app-installation-flow)
6. [App Configuration (Registry)](#app-configuration-registry)
7. [Backup Flow](#backup-flow)
8. [PowerShell Execution Wrapper](#powershell-execution-wrapper)
9. [Downloaders](#downloaders)
10. [System Initialization](#system-initialization)
11. [Self-Install Mechanism](#self-install-mechanism)
12. [Privilege/Elevation Checking](#privilegeelevation-checking)
13. [Tokenizer (Variable Substitution)](#tokenizer-variable-substitution)
14. [Desktop Shortcut Deletion](#desktop-shortcut-deletion)
15. [Logging/Console Output](#loggingconsole-output)
16. [Error Codes](#error-codes)
17. [Known Bugs and Quirks](#known-bugs-and-quirks)

---

## Overview

Configurator is a Windows machine configuration tool that:
- Reads a manifest repo containing a list of apps and per-app definitions
- Installs, upgrades, verifies, and configures each app via PowerShell scripts
- Supports 12 app types with different installation strategies
- Manages its own settings (persisted as JSON in LocalAppData)
- Can initialize a fresh machine by installing prerequisites and cloning the manifest repo
- Can back up app configurations

The PowerShell rewrite must consume the **exact same manifest format** (manifest.json + apps/{AppId}/app.json).

---

## CLI Commands

The CLI requires elevated privileges (administrator). If not elevated, it prints an error and returns exit code 2.

If no arguments are provided, `--help` is shown (exit code 0).

### `initialize`

**Description:** Runs system initialization prerequisites and clones the manifest repo.

**Parameters:** None

**Behavior:**
1. Run `SystemInitializer.InitializeAsync()` (see [System Initialization](#system-initialization))
2. Load settings, read `manifest.repo` setting
3. If `manifest.repo` is null, throw: `"The 'manifest.repo' setting must be set before invoking initialize."`
4. Compute manifest directory: `Path.Combine(git.clonedirectory, repoNameFromUrl)`
   - Repo name extracted from URL: last segment, with `.git` stripped
5. If `manifest.directory` in settings differs from computed value, update settings and save
6. If manifest directory does not exist on disk, clone the repo:
   ```powershell
   Push-Location <git.clonedirectory>
   git clone <manifest.repo>
   Pop-Location
   ```
7. If the manifest file (`manifest.json` by default) does not exist inside the manifest directory:
   - Write `"{ }"` to the file
   - Commit and push:
   ```powershell
   Push-Location <manifest.directory>
   git add .
   git commit -m '[Configurator] Create manifest file'
   git push
   Pop-Location
   ```
8. If everything already exists, do nothing (idempotent).

### `configure-machine` (alias: `configure`)

**Description:** Install/upgrade apps from the manifest.

**Parameters:**
- `--environments` / `-e` (optional): Pipe-separated list of environment names. Parsed by splitting on `|`. Example: `--environments "Work|Personal"`
- `--single-app-id` / `-app` (optional): A single app ID to install. When present, environments are ignored.

**Behavior:**
- If `--single-app-id` is provided:
  1. Load the full manifest (all environments, no filtering)
  2. Find the app matching the ID (first match)
  3. Run install/upgrade + configure for that single app
- Otherwise:
  1. Load the manifest filtered by the specified environments
  2. For each app in order:
     - If the app implements `IDownloadApp`, use `DownloadAppInstaller`
     - Otherwise use `AppInstaller`
     - Then run `AppConfigurator.Configure(app)`

### `settings list`

**Description:** Display all settings as a table.

**Parameters:** None

**Behavior:**
1. Load settings from disk
2. Use reflection to walk the `Settings` object tree
3. For each leaf property (primitive, Uri, or string), collect: Name (dotted path, lowercased), Value (`.ToString()`), Type (type name)
4. Node properties (ManifestSettings, GitSettings) are recursed into with the dotted prefix
5. Output as a table (Name, Value, Type columns)

**Expected settings paths:**
- `downloadsdirectory` (Uri, default: `C:/tmp/configurator-downloads`)
- `manifest.repo` (Uri, default: null)
- `manifest.filename` (String, default: `manifest.json`)
- `manifest.directory` (String, default: null)
- `git.clonedirectory` (Uri, default: `C:\src\`)

### `settings set <setting-name> <setting-value>`

**Description:** Update a single setting by its dotted path name.

**Parameters:**
- `setting-name` (positional argument): The dotted path (e.g., `manifest.repo`)
- `setting-value` (positional argument): The new value

**Behavior:**
1. Load settings
2. Walk the settings tree to find the leaf property matching the dotted path (case-insensitive match via `.ToLower()`)
3. If not found, throw `ArgumentException`: `"{settingName} is not a recognized setting name."`
4. Convert the string value to the target type:
   - `Uri` type: `new Uri(settingValue)`
   - All others: use string directly
5. Set the property value via reflection
6. Save settings

### `add-app` (alias: `add`)

**Description:** Add a new app definition to the manifest.

**Parameters:**
- `--app-id` (string): The app identifier
- `--app-type` (AppType enum): The installer type
- `--environments` (string): Pipe-separated environment list

**Behavior:**
1. Create an `Installable` with the provided values (environments joined with `|`)
2. Call `ManifestRepository.SaveInstallableAsync`:
   - Load the manifest file
   - If the app ID already exists in the manifest's Apps list, return without changes (idempotent)
   - Create the app directory: `<manifest.directory>/apps/<appId>/`
   - Write the app.json file (human-readable JSON, indented)
   - Add the app ID to the manifest's Apps list
   - Write the updated manifest file

### `backup`

**Description:** Run backup scripts for all apps in the manifest.

**Parameters:** None

**Behavior:**
1. Load the full manifest (no environment filtering, empty list passed)
2. For each app in the manifest, call `AppConfigurator.Backup(app)`:
   - Check if the app has a non-empty verification script
   - If it does, run the verification script. If the result is `false` (app not installed), skip.
   - If the app is installed, look for `<manifest.directory>/apps/<appId>/backup.ps1`
   - If the file exists, log `"Backing up {appId}..."`, execute it, log `"Backed up {appId}!"`
   - If the file does not exist or the app is not installed, skip silently.

---

## Settings Management

### Settings Object Structure

```json
{
  "downloadsDirectory": "C:/tmp/configurator-downloads",
  "manifest": {
    "repo": null,
    "fileName": "manifest.json",
    "directory": null
  },
  "git": {
    "cloneDirectory": "C:\\src\\"
  }
}
```

**Default values:**
- `downloadsDirectory`: `C:/tmp/configurator-downloads` (Uri)
- `manifest.repo`: null (Uri, nullable)
- `manifest.fileName`: `manifest.json` (string)
- `manifest.directory`: null (string, nullable - set during initialize)
- `git.cloneDirectory`: `C:\src\` (Uri)

### Persistence Path

Settings are stored at:
```
<LocalAppData>/Configurator/settings.json
```

Where `<LocalAppData>` is `Environment.SpecialFolder.LocalApplicationData` (typically `C:\Users\<user>\AppData\Local`).

### Load Behavior

1. Compute the settings file path: `<LocalAppData>/Configurator/settings.json`
2. If the file does not exist:
   - Create the directory `<LocalAppData>/Configurator/`
   - Write default (empty) settings to the file
3. Ensure the downloads directory exists (create if missing)
4. Read and deserialize the settings file
5. Return the Settings object

### Save Behavior

1. Serialize settings to JSON
2. Write to the settings file path
3. Ensure the downloads directory exists

### JSON Serialization

Two serializers are used:
- **JsonSerializer** (for settings): camelCase property naming, case-insensitive read, enum as camelCase string
- **HumanReadableJsonSerializer** (for manifest/app files): same as above plus `WriteIndented = true`

Both use `System.Text.Json` with:
- `PropertyNameCaseInsensitive = true`
- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
- `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)` for enum handling

---

## Manifest Loading

### manifest.json Format

```json
{
  "apps": [
    "app-id-1",
    "app-id-2",
    "app-id-3"
  ]
}
```

The manifest file also supports a `todo` array (seen in the real machine-configs repo) which is ignored by the tool.

### App Directory Structure

```
<manifest.directory>/
  manifest.json
  apps/
    <app-id>/
      app.json
      install.ps1     (optional, for PowerShell app type)
      upgrade.ps1     (optional, for PowerShell app type)
      verification.ps1 (optional, for PowerShell app type)
      backup.ps1      (optional, for backup command)
```

### app.json Common Fields

Every app.json must have:
```json
{
  "appType": "<AppType>",
  "appId": "<string>",
  "environments": "<pipe-separated-string>"
}
```

### Loading Process

1. Load settings to get `manifest.directory` and `manifest.fileName`
2. Read `<manifest.directory>/<manifest.fileName>` to get the list of app IDs
3. If the manifest file doesn't exist, create it with `{}` content
4. For each app ID, read `<manifest.directory>/apps/<appId>/app.json`
5. Parse the raw JSON to get `AppType`, `AppId`, and `Environments`
6. Filter by specified environments:
   - If no environments specified, include all apps
   - Otherwise, check if any specified environment appears in the app's `Environments` string (case-insensitive `Contains` check)
   - Environment comparison: the app's `Environments` string is lowered, each specified environment is lowered, then `Contains` is used (substring match, not exact word match)
7. Parse each app into its typed object using `AppType` as the discriminator
8. Return `Manifest { AppIds, Apps }`

### Loading a Single App

When loading a single app by ID (for `--single-app-id`):
1. Load the full manifest with no environment filtering
2. Find the first app where `AppId` matches
3. Return that app

### Unknown App Types

Apps with `AppType = Unknown` or any unrecognized type are parsed as `null` and filtered out. They are silently skipped.

### Environment Filtering Details

The `Environments` field in app.json is a pipe-separated string (e.g., `"Environment1|Environment3"`). The filtering uses case-insensitive string `Contains`, not exact word matching. This means an environment filter of `"Env"` would match `"Environment1"`.

---

## App Installation Flow

### AppInstaller.InstallOrUpgradeAsync

This is the core installation logic for all app types.

**Flow:**
1. Log: `"Installing '{appId}'"`
2. Snapshot desktop entries (pre-install)
3. Run verification script (if exists):
   - If `VerificationScript` is null, verification returns `false` (treat as not installed)
   - Otherwise, execute the script and parse result as bool
4. Determine action:
   - If NOT verified (not installed): use `InstallScript`
   - If verified AND `UpgradeScript` is not null AND `PreventUpgrade` is false: use `UpgradeScript`
   - Otherwise: no action (empty string, which is skipped via `IsNullOrWhiteSpace` check)
5. Execute the action script (if non-empty)
6. After execution, run verification again (post-install check)
7. If post-install verification fails: log debug `"Failed to install '{appId}'"`
8. Snapshot desktop entries (post-install)
9. Delete any new desktop entries added during install
10. Log: `"Installed '{appId}'"`

### ForceWindowsPowerShell Variant

PowerShell Core (pwsh.exe) is the default for all script execution. But during initialization, PowerShell Core may not yet be installed. The `IAppInstallerForceWindowsPowerShell` interface uses `ExecuteWindowsAsync` (powershell.exe) instead of `ExecuteAsync` (pwsh.exe) for all operations.

This is used specifically by `PowerShellCoreInstaller` to install PS Core using Windows PowerShell.

### DownloadAppInstaller

For apps implementing `IDownloadApp` (PowerShellAppPackage, VisualStudioExtensionApp, GitHubAssetApp):

1. Log: `"Downloading '{appId}'"`
2. Get the downloader by name from the factory
3. Call `downloader.DownloadAsync(downloaderArgs.ToString())`
4. Set `app.DownloadedFilePath` to the returned path
5. Log: `"Downloaded '{appId}'"`
6. Delegate to `AppInstaller.InstallOrUpgradeAsync(app)` for the actual install

---

## App Configuration (Registry)

After installation, `AppConfigurator.Configure(app)` is called:

1. If `app.Configuration` is null, return immediately
2. For each `RegistrySetting` in `Configuration.RegistrySettings`:
   - Call `RegistryRepository.SetValue(keyName, valueName, valueData)`

### Registry Value Types

The `ValueData` field in a RegistrySetting supports:
- **String**: stored as `RegistryValueKind.String`
- **UInt32** (number in JSON): stored as `RegistryValueKind.DWord`

The `RegistrySettingValueDataConverter` custom JSON converter:
- JSON string -> detokenize via Tokenizer, return string
- JSON number -> `reader.GetUInt32()`, return uint
- Other types -> throw exception

### Registry Operations

**SetValue:**
- Determines `RegistryValueKind` from the .NET type of the value
- For `uint`: converts to `int` (unchecked cast) before writing via `Registry.SetValue`
- Logs errors with full key/value details on failure, then rethrows

**GetValue:**
- Reads via `Registry.GetValue`
- If result is `int`: convert to `uint` via BitConverter
- If result is `long`: convert to `ulong` via BitConverter
- Returns `.ToString()`

**GetSubKeyNames:**
- Opens the key under `HKEY_LOCAL_MACHINE` via `Registry.LocalMachine.OpenSubKey`
- Returns sub-key names, or empty array if key doesn't exist

---

## Backup Flow

`AppConfigurator.Backup(app)`:

1. Check if the app has a non-empty `VerificationScript`
2. If it does, execute it to check if the app is installed
3. If `VerificationScript` is null/empty OR execution returns false -> skip (return early)
4. Load settings to get manifest directory
5. Construct backup script path: `<manifest.directory>/apps/<appId>/backup.ps1`
6. If the file exists:
   - Log: `"Backing up {appId}..."`
   - Execute the backup script via PowerShell
   - Log: `"Backed up {appId}!"`
7. If the file doesn't exist, do nothing

---

## PowerShell Execution Wrapper

All script execution goes through the `PowerShell` class which wraps process execution.

### Execution Modes

| Method | Executable | Use Case |
|--------|-----------|----------|
| `ExecuteAsync(script)` | pwsh.exe | Normal execution via PS Core |
| `ExecuteAdminAsync(script)` | pwsh.exe | Elevated execution via PS Core |
| `ExecuteAsync<T>(script)` | pwsh.exe | Execute and parse last output line |
| `ExecuteWindowsAsync(script)` | powershell.exe | Windows PowerShell execution |
| `ExecuteWindowsAdminAsync(script)` | powershell.exe | Elevated Windows PowerShell |
| `ExecuteWindowsAsync<T>(script)` | powershell.exe | Windows PS with result |

### Environment-Ready Script Wrapping

Every script is wrapped with environment setup before execution:

```powershell
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

if ($profile -eq $null -or $profile -eq '') {
  $global:profile = "<MyDocuments>/<PSFolderName>/Microsoft.PowerShell_profile.ps1"
}

<actual script>
```

Where `<PSFolderName>` is:
- `"PowerShell"` for PS Core (pwsh.exe)
- `"WindowsPowerShell"` for Windows PowerShell (powershell.exe)

This ensures:
1. PATH is rebuilt from Machine + User environment variables (not inherited from parent process)
2. `$profile` is set correctly if not already defined

### Script-to-File Conversion

Scripts are not passed inline. They are:
1. Written to a temp file at `<LocalAppData>/Configurator/temp/<timestamp>.ps1`
2. Timestamp format: `yyyy-MM-dd_HH-mm-ss-fffff`
3. Executed via `-File <path>`

### pwsh.exe Discovery

PS Core's install location is found via the Windows Registry:
1. Read sub-keys of `SOFTWARE\Microsoft\PowerShellCore\InstalledVersions`
2. Take the first sub-key (GUID)
3. Read `InstallLocation` value from `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{GUID}`
4. Executable path: `{InstallLocation}pwsh.exe`

If no installed versions are found, throw: `"PowerShell Core is not installed. No versions found in registry."`

### Process Execution

The `ProcessRunner` launches processes with:
- `UseShellExecute = RunAsAdmin` (true for admin, false for normal)
- `RedirectStandardOutput = !RunAsAdmin`
- `RedirectStandardError = !RunAsAdmin`
- `Verb = RunAsAdmin ? "runas" : "open"`
- `FileName = <executable>`
- `Arguments = -File <scriptFilePath>`

For admin execution: output/error cannot be captured (shell execute mode).

**Output handling:**
- Each stdout line is added to `AllOutput` and logged via `Debug`
- `LastOutput` tracks the most recent output line
- Each stderr line is cleaned of ANSI escape sequences and added to `Errors`, logged via `Debug`

**Error handling:**
- All error strings are logged via `consoleLogger.Error`
- If exit code != 0, throw: `"Script failed to complete with exit code {exitCode}"`

### Result Type Mapping

When a typed result is expected (`ExecuteAsync<T>`):
- `string`: return `LastOutput` as-is
- `bool`: parse `LastOutput` via `bool.Parse()` (supports True/False, case-insensitive)
- Other types: throw `NotSupportedException`
- If `LastOutput` is null: return `default(T)`

---

## Downloaders

### DownloaderFactory

Resolves downloader by name using reflection:
1. Construct type name: `Configurator.Downloaders.{downloaderName}`
2. Resolve type via `Type.GetType`
3. If type not found, throw with message containing the name and namespace
4. Resolve instance from DI container

### GitHubAssetDownloader

**DownloaderArgs format:**
```json
{
  "User": "dannydwarren",
  "Repo": "configurator",
  "Extension": ".exe"
}
```

**Flow:**
1. Deserialize args JSON
2. Execute PowerShell script to query GitHub API:
   ```powershell
   $asset = (iwr https://api.github.com/repos/{User}/{Repo}/releases/latest | ConvertFrom-Json).assets | ? { $_.name -like '*{Extension}' }
   $downloadUrl = $asset | select -exp browser_download_url
   $fileName = $asset | select -exp name
   Write-Output "{ `"FileName`": `"$fileName`", `"Url`": `"$downloadUrl`" }"
   ```
3. Parse the JSON output to get `FileName` and `Url`
4. Download the file via `ResourceDownloader`
5. Return the local file path

### VisualStudioMarketplaceDownloader

**DownloaderArgs format:**
```json
{
  "Publisher": "Shanewho",
  "ExtensionName": "IHateRegions"
}
```

**Flow:**
1. Deserialize args JSON
2. HTTP GET the extension page: `https://marketplace.visualstudio.com/items?itemName={Publisher}.{ExtensionName}`
3. Regex-match the versioned download URL from the page HTML:
   ```
   /_apis/public/gallery/publishers/{Publisher}/vsextensions/{ExtensionName}/(\d+\.?)+/vspackage
   ```
4. Build full download URL: `https://marketplace.visualstudio.com{matchedPath}`
5. Download file as `{Publisher}.{ExtensionName}.vsix`
6. Return the local file path

### ResourceDownloader

Downloads a file from a URL:
1. HTTP GET the URL
2. If status code != 200, throw: `"Failed with status code {code} to download {fileName}"`
3. Write the stream to `<downloadsDirectory>/{fileName}`
4. Return the full file path

---

## System Initialization

`SystemInitializer.InitializeAsync()` runs these steps **in order**:

1. **Set Windows PowerShell Execution Policy**
   - Execute `Set-ExecutionPolicy RemoteSigned -Force` via Windows PowerShell (admin)
   - Report the policy result via `Get-ExecutionPolicy`
   - Report the version via `$PSVersionTable.PSVersion.ToString()`

2. **Upgrade Winget**
   - Execute via Windows PowerShell:
     ```powershell
     Add-AppxPackage https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle -ForceTargetApplicationShutdown
     ```
     ```powershell
     Add-AppxPackage https://cdn.winget.microsoft.com/cache/source.msix
     ```

3. **Accept Winget Source Agreements**
   - Execute via Windows PowerShell:
     ```powershell
     winget list winget --accept-source-agreements
     ```

4. **Install PowerShell Core**
   - Install `Microsoft.PowerShell` via Winget using **Windows PowerShell** (ForceWindowsPowerShell)
   - This uses the `AppInstaller` flow with `WingetApp { AppId = "Microsoft.PowerShell" }`

5. **Set PowerShell Core Execution Policy**
   - Execute `Set-ExecutionPolicy RemoteSigned -Force` via PS Core (admin)
   - Report policy and version via PS Core

6. **Self-Install** (see [Self-Install Mechanism](#self-install-mechanism))

7. **Install Scoop CLI**
   - Uses `ScriptApp` with:
     - Install: `iwr get.scoop.sh -OutFile $env:tmp\scoop-install.ps1` then `& $env:tmp\scoop-install.ps1 -RunAsAdmin`
     - Verification: test if `scoop` command exists via `Get-Command`

8. **Install Git**
   - Install `Git.Git` via Winget (via normal `AppInstaller`)

9. **Install Manifest Repo**
   - Load settings to get `manifest.repo` URL
   - If `manifest.repo` is null, throw `ArgumentException: "Missing setting: Manifest.Repo"`
   - Install as `GitRepoApp { AppId = "git.manifest-repo", InstallArgs = repoUrl, CloneRootDirectory = git.cloneDirectory }`

---

## Self-Install Mechanism

`SelfInstaller.InstallAsync()`:

1. Check if `c:\Configurator` directory exists
2. If it exists, return immediately (already installed)
3. Set up a `GitHubAssetApp` with:
   - AppId: `"Configurator"`
   - DownloaderArgs: `{ "User": "dannydwarren", "Repo": "configurator", "Extension": ".exe" }`
4. Download and install via `DownloadAppInstaller`
5. Create `c:\Configurator` directory
6. Move the downloaded file to `c:\Configurator\Configurator.exe`
7. Add `c:\Configurator` to the machine PATH environment variable

**NOTE for PowerShell rewrite:** This mechanism downloads the C# executable from GitHub releases. For the PowerShell version, this needs to be reimagined - likely downloading/installing the PowerShell module instead.

---

## Privilege/Elevation Checking

Before any CLI command executes:
1. Check if the current user is running as Administrator
2. Uses `WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)`
3. If not elevated:
   - Log error: `"Configurator Cli must be run with elevated privileges."`
   - Return exit code `2` (`ErrorCode.NotEnoughPrivileges`)

---

## Tokenizer (Variable Substitution)

The Tokenizer replaces `{{source:name}}` patterns in strings with actual values.

### Token Pattern

```
{{source:name}}
```

- Matched via regex: `(\{\{(.*?)\}\})+`
- Source and name extracted by trimming `{` and `}`, splitting on `:`

### Supported Sources

| Source | Resolution |
|--------|-----------|
| `env` | `Environment.GetEnvironmentVariable(name)` (empty string if null) |

### Usage

Used exclusively in `RegistrySetting.ValueData` deserialization. When a registry setting's ValueData is a string, the Tokenizer replaces any tokens before the value is stored.

**Example:**
```json
{
  "KeyName": "key-2",
  "ValueName": "string",
  "ValueData": "{{env:ProgramFiles}}\\string-data"
}
```
Resolves to: `"C:\Program Files\string-data"`

---

## Desktop Shortcut Deletion

During app installation, desktop shortcuts are automatically cleaned up:

1. Before install: enumerate all files/directories on the user's desktop and common desktop
2. After install: enumerate again
3. Compute the difference (new entries)
4. Delete all new entries

### Desktop Paths

Two desktop locations are checked:
- User desktop: `Environment.SpecialFolder.Desktop`
- Common (public) desktop: `Environment.SpecialFolder.CommonDesktopDirectory`

Both paths are enumerated for file system entries. Entries are deduplicated.

### Deletion

- If the path is a file, delete it
- If the path is a directory, delete it (non-recursive in C# implementation)

---

## Logging/Console Output

### Log Format

```
[<ISO8601 timestamp>] [<LEVEL>] <message>
```

Timestamp uses the `O` format specifier (e.g., `2024-01-15T10:30:45.1234567-07:00`).

### Log Levels and Colors

| Level | Label | Color |
|-------|-------|-------|
| Debug | `DEBUG` | Gray |
| Verbose | `INFOV` | White (default) |
| Info | `INFO ` | White (default) |
| Warn | `WARN ` | Yellow |
| Error | `ERROR` | Red |
| Progress | `PRGRS` | Blue |
| Result | `RESLT` | Green |

### Table Output

The `settings list` command outputs a table using the `ConsoleTables` library with `Format.Minimal` style. In PowerShell this can be replicated with `Format-Table`.

---

## Error Codes

| Code | Name | Description |
|------|------|-------------|
| 0 | Success | (implicit) |
| 1 | GenericFailure | Invalid CLI arguments or general failure |
| 2 | NotEnoughPrivileges | CLI not run as administrator |

---

## Known Bugs and Quirks

### GitHubAssestApp Typo

The C# class is named `GitHubAssestApp` (misspelled "Asset"). The PowerShell version should fix this to `GitHubAssetApp`. However, the `AppType` enum does NOT include a `GitHubAsset` entry. The `GitHubAssestApp` is used directly in `SelfInstaller` but is never parsed from manifests via `AppType`. It is created programmatically only.

**Recommendation:** In the PowerShell version, use `GitHubAssetApp` (corrected spelling) but keep it as an internal type not exposed via manifest AppType.

### ScoopBucketApp VerificationScript Bug

The C# `ScoopBucketApp.VerificationScript` has a bug:
```csharp
public string VerificationScript => @"(scoop bucket list | Select-String {AppId}) -ne $null";
```
The `{AppId}` is NOT interpolated as a C# string interpolation - it's a verbatim string. This means the literal text `{AppId}` is sent to PowerShell, which would be interpreted as a PowerShell scriptblock, not the actual app ID. This is a bug in the C# version.

**Recommendation:** Fix this in the PowerShell version to properly interpolate the app ID.

### VisualStudioExtensionApp InstallScript Typo

The install script contains `$vsi0xInstaller` (with a zero instead of lowercase 'x'):
```csharp
Start-Process $vsi0xInstaller $installArgs -Wait
```
The variable is assigned as `$vsixInstaller` but referenced as `$vsi0xInstaller`. This would cause a runtime error.

**Recommendation:** Fix to `$vsixInstaller` in the PowerShell version.

### NonPackageApp InstallScript Convention

The `NonPackageApp` generates its install script as `./{appId}_install.ps1`. This is a relative path, meaning it depends on the current working directory at execution time. The behavior is fragile.

### GitRepoApp Null Return

When `GitRepoApp.InstallArgs` is null or whitespace, `ParseGitRepoApp` returns `null`. These null apps are filtered out by the `.Where(x => x != null)` in manifest loading. This means Git repo apps without InstallArgs are silently dropped.

### PowerShellApp with Missing install.ps1

If a PowerShell app's `install.ps1` file doesn't exist in the app directory, `MapShellAppScripts` returns `default(T)` which is null. These are filtered out during manifest loading. The app is silently dropped.

### Environment Filtering is Substring-Based

The environment filtering uses `Contains` (case-insensitive) rather than exact word matching. An environment filter of `"dev"` would match an app with `Environments = "Development"`.

### Dependency Injection

The C# version uses Scrutor for assembly scanning DI registration (`services.Scan`). All classes are registered as transient, both as themselves and as matching interfaces. A special registration maps `IAppInstallerForceWindowsPowerShell` to `AppInstaller`.

The `RegistrySettingValueDataConverter.Tokenizer` is set via a static property after DI initialization. This is a workaround for the JSON converter not participating in DI.
