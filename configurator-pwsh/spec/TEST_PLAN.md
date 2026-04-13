# Test Plan

This document describes the tests that must exist for the PowerShell Configurator rewrite, derived from the C# unit and integration test suites.

---

## Test Framework

Use **Pester** (PowerShell testing framework) for both unit and integration tests.

For mocking, use Pester's built-in `Mock` capabilities. The C# tests use AutoMoq for auto-mocking dependencies; in PowerShell, we mock at the function/cmdlet level.

---

## Unit Tests

### CLI Tests

**File:** `Cli.Tests.ps1`

| Test | Description |
|------|-------------|
| Invalid commandline arg | Returns exit code 1 (GenericFailure) |
| No elevated privileges | Returns exit code 2, logs error: "Configurator Cli must be run with elevated privileges." |
| No commandline args | Shows help, returns exit code 0 |
| `initialize` command | Resolves and executes the initialize command, returns 0 |
| `settings set <name> <value>` command | Resolves and executes set-setting with both args, returns 0 |
| `settings list` command | Resolves and executes list-settings, returns 0 |
| `add-app` with `--app-id`, `--app-type`, `--environments` | Parses pipe-separated environments, executes add-app |
| `add` (alias) | Same as `add-app` |
| `configure-machine` | Executes configure with empty environment list and null singleAppId |
| `configure` (alias) | Same as `configure-machine` |
| `configure --single-app-id <id>` | Passes the single app ID |
| `configure -app <id>` | Same as `--single-app-id` |
| `configure --environments <envs>` | Parses environments |
| `configure -e <envs>` | Same as `--environments` |
| `backup` command | Resolves and executes backup command, returns 0 |

### ConfigureMachineCommand Tests

**File:** `ConfigureMachineCommand.Tests.ps1`

| Test | Description |
|------|-------------|
| Installing all manifest apps | Loads manifest, installs each app (IDownloadApp via DownloadAppInstaller, others via AppInstaller), configures each |
| Installing single manifest app | Loads single app by ID, installs and configures it |

### BackupMachineCommand Tests

**File:** `BackupMachineCommand.Tests.ps1`

| Test | Description |
|------|-------------|
| Backing up all apps | Loads full manifest (empty environments), calls Backup for each app |

### InitializeCommand Tests

**File:** `InitializeCommand.Tests.ps1`

| Test | Description |
|------|-------------|
| First-time initialization | Calls SystemInitializer, sets manifest directory, clones repo, creates manifest file |
| With new manifest repo | Creates, commits, and pushes manifest file when it doesn't exist |
| Already initialized | Does nothing when manifest dir and file already exist |
| Missing manifest.repo setting | Throws with message: "The 'manifest.repo' setting must be set before invoking initialize." |

### SystemInitializer Tests

**File:** `SystemInitializer.Tests.ps1`

| Test | Description |
|------|-------------|
| Full initialization flow | Verifies all 9 steps execute in order: Windows PS exec policy, winget upgrade, winget source agreements, PS Core install, PS Core exec policy, self-install, scoop-cli, git, manifest repo |

### AppInstaller Tests

**File:** `AppInstaller.Tests.ps1`

| Test | Description |
|------|-------------|
| Install (not yet installed) | Runs verification (false), runs install script, runs post-verification, deletes new desktop shortcuts |
| Force Windows PowerShell | Uses ExecuteWindowsAsync variants instead of ExecuteAsync |
| No verification script | Skips verification, runs install script directly |
| Nothing added to desktop | Does not call desktop delete |
| Upgrade (already installed) | Verification returns true, runs upgrade script, deletes new desktop shortcuts |
| Prevent upgrade | Verification returns true, PreventUpgrade=true, does NOT run upgrade script |
| Already installed, no upgrade script | Verification returns true, no upgrade script, does nothing |
| PowerShell shell app | Same flow but with .ps1 file-based scripts |

### AppConfigurator Tests

**File:** `AppConfigurator.Tests.ps1`

| Test | Description |
|------|-------------|
| Configure with registry settings | Sets all provided registry values |
| No configuration provided | Does nothing |
| Default configuration (empty registry list) | Does nothing |
| Backup an installed app | Verifies app is installed, finds and executes backup.ps1, logs |
| Backup without verification script | Skips (no verification = not verifiable) |
| Backup not installed app | Verification returns false, skips |
| Backup without backup script | App is installed but no backup.ps1 exists, skips |

### DownloadAppInstaller Tests

**File:** `DownloadAppInstaller.Tests.ps1`

| Test | Description |
|------|-------------|
| Download and install | Gets downloader from factory, downloads, sets DownloadedFilePath, delegates to AppInstaller |

### AddAppCommand Tests

**File:** `AddAppCommand.Tests.ps1`

| Test | Description |
|------|-------------|
| Add Winget app | Creates Installable with correct AppId, AppType, pipe-joined Environments, saves |
| Add Scoop app | Same flow, different AppType |

### SelfInstaller Tests

**File:** `SelfInstaller.Tests.ps1`

| Test | Description |
|------|-------------|
| Install | Downloads Configurator from GitHub, moves to c:\Configurator\, adds to machine PATH |
| Already installed | c:\Configurator exists, does nothing |

### ManifestRepoInstaller Tests

**File:** `ManifestRepoInstaller.Tests.ps1`

| Test | Description |
|------|-------------|
| Install | Creates GitRepoApp with manifest repo URL, installs via AppInstaller |
| Missing repo setting | Throws ArgumentException with message about missing Manifest.Repo |

### PowerShellCoreInstaller Tests

**File:** `PowerShellCoreInstaller.Tests.ps1`

| Test | Description |
|------|-------------|
| Install | Installs Microsoft.PowerShell WingetApp via ForceWindowsPowerShell |

### ScoopCliInstaller Tests

**File:** `ScoopCliInstaller.Tests.ps1`

| Test | Description |
|------|-------------|
| Install | Installs ScriptApp with scoop install/verification scripts |

### GitInstaller Tests

**File:** `GitInstaller.Tests.ps1`

| Test | Description |
|------|-------------|
| Install | Installs Git.Git WingetApp via AppInstaller |

### WingetConfiguration Tests

**File:** `WingetConfiguration.Tests.ps1`

| Test | Description |
|------|-------------|
| Upgrade | Executes two Add-AppxPackage commands via Windows PowerShell |
| Accept source agreements | Executes winget list with --accept-source-agreements via Windows PowerShell |

### PowerShellConfiguration Tests

**File:** `PowerShellConfiguration.Tests.ps1`

| Test | Description |
|------|-------------|
| Set PS Core execution policy | Runs Set-ExecutionPolicy via admin, reports policy and version |
| Set Windows PS execution policy | Same but via Windows PowerShell |

### PowerShell Execution Tests

**File:** `PowerShellExecution.Tests.ps1`

| Test | Description |
|------|-------------|
| Execute Core | Wraps script with env setup, saves to file, executes via pwsh.exe |
| Execute Windows | Same but with powershell.exe and WindowsPowerShell profile path |
| Execute as admin (Core) | Uses RunAsAdmin=true, pwsh.exe |
| Execute as admin (Windows) | Uses RunAsAdmin=true, powershell.exe |
| Unsuccessful exit code | Throws with message containing exit code |
| Execute with errors | Logs all errors, throws |
| Execute with string result | Returns LastOutput as string |
| Execute with bool result (True/False/true/false/TRUE/FALSE) | Parses bool correctly |

### Settings Tests

**File:** `Settings.Tests.ps1`

| Test | Description |
|------|-------------|
| ListSettingsCommand | Loads settings, maps to table rows with Name/Value/Type, outputs table |
| SetSettingCommand - manifest.repo | Updates Uri property, saves |
| SetSettingCommand - manifest.filename | Updates string property, saves |
| SetSettingCommand - git.clonedirectory | Updates Uri property, saves |
| SettingsRepository - load existing | Reads and deserializes settings file |
| SettingsRepository - first load | Creates directory, writes defaults, ensures downloads dir |
| SettingsRepository - save | Serializes and writes, ensures downloads dir |

### Utility Tests

**File:** `Utilities.Tests.ps1`

| Test | Description |
|------|-------------|
| DesktopRepository - load entries | Enumerates both desktop paths, deduplicates |
| DesktopRepository - delete paths | Deletes each path |
| ScriptToFileConverter | Creates temp dir, writes script to timestamped .ps1 file, returns path |
| Tokenizer - single env token | `{{env:ProgramFiles}}\path` -> `C:\Program Files\path` |
| Tokenizer - multiple env tokens | Replaces all tokens in string |
| ResourceDownloader - success | HTTP GET, write stream, return path |
| ResourceDownloader - failure | Non-200 status throws |

### Downloader Tests

**File:** `Downloaders.Tests.ps1`

| Test | Description |
|------|-------------|
| DownloaderFactory - get by name | Resolves downloader type by name from namespace |
| DownloaderFactory - unknown name | Throws with message containing name and namespace |
| GitHubAssetDownloader | Deserializes args, executes PS script to query GitHub API, parses result, downloads |
| VisualStudioMarketplaceDownloader | Fetches page HTML, regex-matches download URL, downloads |

---

## Integration Tests

### ManifestRepository Integration Tests

**File:** `ManifestRepository.Integration.Tests.ps1`

These tests use actual file I/O with test manifest directories.

| Test | Description | Test Manifest |
|------|-------------|---------------|
| Save to new manifest | Creates manifest file, app directory, app.json | dynamic |
| Save to existing manifest | Adds second app while preserving first | dynamic |
| Save duplicate (idempotent) | Same AppId saved twice, no changes | dynamic |
| Load single app | Loads specific app by ID | `multiple-apps.manifest.json` |
| Load Gitconfig apps | Parses GitconfigApp correctly | `gitconfig.manifest.json` |
| Load GitRepo apps | Parses GitRepoApp with scripts containing clone dir, excludes misconfigured | `git-repo.manifest.json` |
| Load NonPackage apps | Parses NonPackageApp | `non-package.manifest.json` |
| Load PowerShell apps | Parses with file-based scripts (install, upgrade, verification) | `powershell.manifest.json` |
| Load PowerShell apps (missing non-install scripts) | Upgrade/verification null when files missing | `powershell.manifest.json` |
| Load PowerShell apps (missing install script) | App excluded from manifest | `powershell.manifest.json` |
| Load PowerShellAppPackage apps | Parses with downloader fields, preventUpgrade | `power-shell-app-packages.manifest.json` |
| Load PowerShellModule apps (basic) | Parses with empty installArgs | `power-shell-module.manifest.json` |
| Load PowerShellModule apps (with installArgs) | InstallArgs = " install-args" | `power-shell-module.manifest.json` |
| Load PowerShellModule apps (with preventUpgrade) | PreventUpgrade = true | `power-shell-module.manifest.json` |
| Load PowerShellModule apps (with configuration) | Registry settings parsed | `power-shell-module.manifest.json` |
| Load Scoop apps (basic) | Parses correctly | `scoop.manifest.json` |
| Load Scoop apps (with installArgs) | " install-args" | `scoop.manifest.json` |
| Load Scoop apps (with preventUpgrade) | true | `scoop.manifest.json` |
| Load Scoop apps (with configuration) | Registry settings | `scoop.manifest.json` |
| Load ScoopBucket apps | Parses correctly | `scoop-bucket.manifest.json` |
| Load Script apps (basic) | InstallScript, VerificationScript, UpgradeScript | `script.manifest.json` |
| Load Script apps (with configuration) | Registry settings | `script.manifest.json` |
| Load VisualStudioExtension apps | DownloaderArgs with Publisher and ExtensionName | `visual-studio-extension.manifest.json` |
| Load Winget apps (basic) | Empty installArgs, no preventUpgrade | `winget.manifest.json` |
| Load Winget apps (with installArgs) | " --override install-args" | `winget.manifest.json` |
| Load Winget apps (with preventUpgrade) | true | `winget.manifest.json` |
| Load Winget apps (with configuration) | Registry settings | `winget.manifest.json` |
| Load Unknown apps | Excluded (empty list) | `unknown.manifest.json` |
| Load with registry settings (string) | String ValueData | `registry-settings.manifest.json` |
| Load with registry settings (tokenized string) | `{{env:ProgramFiles}}` replaced | `registry-settings.manifest.json` |
| Load with registry settings (uint) | UInt32 ValueData | `registry-settings.manifest.json` |
| Load with no specified environments | All apps loaded | `multiple-apps.manifest.json` |
| Load for specific environment | Only matching apps | `multiple-apps.manifest.json` |
| Load for multiple environments | Apps matching any | `multiple-apps.manifest.json` |

### Settings Integration Tests

**File:** `Settings.Integration.Tests.ps1`

| Test | Description |
|------|-------------|
| InMemorySettingsRepository - load before save | Returns sensible test defaults |
| InMemorySettingsRepository - load after save | Returns saved settings |

### DependencyBootstrapper Integration Tests

**File:** `DependencyBootstrapper.Integration.Tests.ps1`

| Test | Description |
|------|-------------|
| Initialize | Static dependencies (Tokenizer) are set up correctly |

### ProcessRunner Integration Tests

**File:** `ProcessRunner.Integration.Tests.ps1`

| Test | Description |
|------|-------------|
| Basic execution | `Write-Host 'Hello World'` exits 0 |
| JSON output | Multi-variable script outputs valid JSON, parseable to object |
| Admin execution | RunAsAdmin=true, exit code 0, no captured output |
| Output capture | Multiple Write-Output lines captured in AllOutput, LastOutput = last line |
| Error capture | Write-Error lines captured in Errors, cleaned of ANSI sequences |

### Registry Integration Tests

**File:** `Registry.Integration.Tests.ps1`

| Test | Description |
|------|-------------|
| Get registry value | Reads known Windows registry value |
| Set string registry value | Writes and reads back a string value |
| Set uint32 registry value | Writes and reads back a large uint32 value |

---

## Test Manifests to Recreate

The following test manifest files and app.json structures must be recreated for integration tests. These are copied from `Configurator.IntegrationTests/TestManifests/`.

### Manifest Files

| File | Content (App IDs) |
|------|-------------------|
| `winget.manifest.json` | winget-app-id, winget-app-id-with-install-args, winget-app-id-with-prevent-upgrade, winget-app-id-with-configuration |
| `scoop.manifest.json` | scoop-app-id, scoop-app-id-with-install-args, scoop-app-id-with-prevent-upgrade, scoop-app-id-with-configuration |
| `scoop-bucket.manifest.json` | scoop-bucket-app-id |
| `script.manifest.json` | script-app-id, script-app-id-with-configuration |
| `powershell.manifest.json` | powershell-app-id-1, powershell-app-id-2, powershell-app-id-3 |
| `power-shell-app-packages.manifest.json` | power-shell-app-package-app-id, power-shell-app-package-app-id-with-prevent-upgrade |
| `power-shell-module.manifest.json` | power-shell-module-app-id, power-shell-module-app-id-with-install-args, power-shell-module-app-id-with-prevent-upgrade, power-shell-module-app-id-with-configuration |
| `gitconfig.manifest.json` | gitconfig-app-id |
| `git-repo.manifest.json` | git-repo-app-id, git-repo-app-id-missing-install-args, git-repo-app-id-blank-install-args |
| `non-package.manifest.json` | non-package-app-id |
| `visual-studio-extension.manifest.json` | visual-studio-extension-app-id |
| `unknown.manifest.json` | unknown-app-id |
| `multiple-apps.manifest.json` | environment-1-app-id-1, environment-1-app-id-2, environment-1-and-3-app-id-3, environment-2-app-id-1, environment-3-app-id-1 |
| `registry-settings.manifest.json` | registry-settings-app-id |

### PowerShell App Script Files

- `apps/powershell-app-id-1/install.ps1` (exists, content not important for parsing test)
- `apps/powershell-app-id-1/upgrade.ps1` (exists)
- `apps/powershell-app-id-1/verification.ps1` (exists)
- `apps/powershell-app-id-2/install.ps1` (exists, no upgrade/verification)
- `apps/powershell-app-id-3/` (directory exists, NO install.ps1)

---

## What the C# Tests Cover vs. What's Missing

### Well Covered

- CLI argument parsing and routing
- Settings load/save/list/set flows
- App installation decision logic (verify -> install/upgrade)
- Desktop shortcut cleanup
- Download app flow
- All app type manifest parsing (integration tests)
- Environment filtering
- Registry setting types (string, uint, tokenized string)
- PowerShell execution wrapping (Core and Windows variants)
- Self-installer download and move logic
- System initialization step ordering
- Error handling (privileges, missing settings, bad exit codes)

### Not Covered / Gaps

- **No end-to-end tests**: No tests that run the full CLI -> command -> install flow against real packages
- **No actual PowerShell execution in unit tests**: All PS execution is mocked
- **ScoopBucketApp verification bug**: The bug in the verification script is not caught by tests because the script is never actually executed (only mocked)
- **VisualStudioExtensionApp install script bug**: The `$vsi0xInstaller` typo is not caught
- **NonPackageApp relative path**: No test validates the working directory assumption
- **Concurrent manifest writes**: No test for race conditions on manifest file updates
- **Large manifest performance**: No test with 100+ apps (the real manifest has 127)
- **Network failure handling**: GitHub API and VS Marketplace download failures are minimally tested
- **ProcessRunner ANSI cleaning**: Only implicitly tested via integration tests
