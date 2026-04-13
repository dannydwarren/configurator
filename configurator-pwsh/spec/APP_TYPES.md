# App Types - Detailed Specification

This document describes every app type supported by Configurator, including their JSON format, generated scripts, and special behaviors.

---

## Common Fields (All App Types)

Every `app.json` file requires these fields:

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `appType` | string (enum) | Yes | The app type discriminator |
| `appId` | string | Yes | Unique identifier for the app |
| `environments` | string | Yes | Pipe-separated environment names (e.g., `"Work\|Personal"`) |

---

## AppType Enum Values

```
Unknown, Script, PowerShell, PowerShellAppPackage, PowerShellModule,
Winget, Scoop, ScoopBucket, Gitconfig, NonPackageApp,
VisualStudioExtension, GitRepo
```

JSON serialization uses camelCase: `"winget"`, `"scoop"`, `"scoopBucket"`, `"powerShell"`, `"powerShellModule"`, `"powerShellAppPackage"`, `"script"`, `"gitconfig"`, `"nonPackageApp"`, `"visualStudioExtension"`, `"gitRepo"`, `"unknown"`

---

## 1. Winget

Package manager app installed via Windows Package Manager (winget).

### app.json

```json
{
  "appType": "winget",
  "appId": "Microsoft.VisualStudioCode",
  "environments": "Work|Personal",
  "installArgs": "optional-override-args",
  "preventUpgrade": false,
  "configuration": {
    "registrySettings": [
      {
        "keyName": "HKEY_CURRENT_USER\\SOFTWARE\\...",
        "valueName": "SomeSetting",
        "valueData": "SomeValue"
      }
    ]
  }
}
```

### Optional Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `installArgs` | string | `""` | If provided, prepended with ` --override `. If null/whitespace, empty string. |
| `preventUpgrade` | bool | `false` | If true, skip upgrade even when already installed |
| `configuration` | object | null | Registry settings to apply after install |

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `winget install --id {AppId} --accept-package-agreements -h -e{InstallArgs}` |
| Verify | `(winget list --id {AppId} -e \| Select-String {AppId}) -ne $null` |
| Upgrade | `winget upgrade --id {AppId} --accept-package-agreements -h -e{InstallArgs}` |

### InstallArgs Behavior

The `InstallArgs` setter transforms the value:
- If null/whitespace: stored as `""` (empty)
- Otherwise: stored as `" --override {value}"`

This means the raw app.json value `"install-args"` becomes `" --override install-args"` in the script.

---

## 2. Scoop

Package installed via the Scoop package manager.

### app.json

```json
{
  "appType": "scoop",
  "appId": "7zip",
  "environments": "Personal",
  "installArgs": "optional-extra-args",
  "preventUpgrade": false,
  "configuration": null
}
```

### Optional Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `installArgs` | string | `""` | Extra args, prepended with a space if provided |
| `preventUpgrade` | bool | `false` | If true, skip upgrade |
| `configuration` | object | null | Registry settings |

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `scoop install {AppId}{InstallArgs}` |
| Verify | `(scoop export \| Select-String {AppId}) -ne $null` |
| Upgrade | `scoop update {AppId}` |

### InstallArgs Behavior

- If null/whitespace: stored as `""` (empty)
- Otherwise: stored as `" {value}"`

---

## 3. ScoopBucket

Adds a Scoop bucket (repository of package definitions).

### app.json

```json
{
  "appType": "scoopBucket",
  "appId": "extras",
  "environments": "Personal"
}
```

### Fixed Properties

| Property | Value |
|----------|-------|
| InstallArgs | null |
| PreventUpgrade | false |
| UpgradeScript | null |
| Configuration | null |

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `scoop bucket add {AppId}` |
| Verify | `(scoop bucket list \| Select-String {AppId}) -ne $null` |
| Upgrade | null (not upgradeable) |

### BUG NOTE

The C# verification script has a bug: `{AppId}` is inside a verbatim string literal and is NOT interpolated. The PowerShell version should fix this to properly include the app ID.

---

## 4. PowerShell

App installed via external PowerShell script files located in the app's manifest directory.

### app.json

```json
{
  "appType": "powerShell",
  "appId": "my-custom-app",
  "environments": "Work"
}
```

### Script Resolution

The PowerShell app type does NOT define scripts inline in app.json. Instead, scripts are loaded from files in the app directory:

| Script | File | Requirement |
|--------|------|-------------|
| Install | `<manifest>/apps/<appId>/install.ps1` | **Required** - app is dropped if missing |
| Upgrade | `<manifest>/apps/<appId>/upgrade.ps1` | Optional - null if missing |
| Verify | `<manifest>/apps/<appId>/verification.ps1` | Optional - null if missing |

### Script Format

Scripts are dot-sourced:
- Install: `. "<manifest>/apps/<appId>/install.ps1"`
- Upgrade: `. "<manifest>/apps/<appId>/upgrade.ps1"`
- Verification: `. "<manifest>/apps/<appId>/verification.ps1"`

### Fixed Properties

| Property | Value |
|----------|-------|
| Shell | PowerShell |
| InstallArgs | null |
| PreventUpgrade | false |
| Configuration | null |

### Special Behaviors

- If `install.ps1` does not exist: the entire app is silently dropped from the manifest (returns null from parser)
- If `upgrade.ps1` or `verification.ps1` don't exist: the corresponding script property is null
- The `Shell` property is set to `Shell.PowerShell` which triggers the file-based script loading path

---

## 5. PowerShellModule

PowerShell module installed via `Install-Module` / `Update-Module`.

### app.json

```json
{
  "appType": "powerShellModule",
  "appId": "posh-git",
  "environments": "Work",
  "installArgs": "-Force -AllowClobber",
  "preventUpgrade": false,
  "configuration": null
}
```

### Optional Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `installArgs` | string | `""` | Extra args, prepended with space |
| `preventUpgrade` | bool | `false` | If true, skip upgrade |
| `configuration` | object | null | Registry settings |

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `Import-Module PowerShellGet -UseWindowsPowerShell\nInstall-Module -Name {AppId}{InstallArgs}` |
| Verify | `(Get-Module -ListAvailable {AppId}) -ne $null` |
| Upgrade | `Import-Module PowerShellGet -UseWindowsPowerShell\nUpdate-Module -Name {AppId}{InstallArgs}` |

Note: Install and Upgrade scripts are multi-line (contain `\n`).

### InstallArgs Behavior

Same as Scoop: prepended with space if non-empty.

---

## 6. PowerShellAppPackage

App package (MSIX/Appx) installed via `Add-AppPackage` after downloading.

### app.json

```json
{
  "appType": "powerShellAppPackage",
  "appId": "Microsoft.DesktopAppInstaller",
  "environments": "Work",
  "preventUpgrade": false,
  "downloader": "GitHubAssetDownloader",
  "downloaderArgs": {
    "User": "microsoft",
    "Repo": "winget-cli",
    "Extension": ".msixbundle"
  }
}
```

### Fields

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `preventUpgrade` | bool | No | false | Prevent upgrade |
| `downloader` | string | Yes | - | Name of the downloader class |
| `downloaderArgs` | object | Yes | - | JSON passed to the downloader |

### Implements IDownloadApp

This type goes through `DownloadAppInstaller` which downloads the file first, then installs.

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `Import-Module appx -UseWindowsPowerShell\nAdd-AppPackage {DownloadedFilePath}` |
| Verify | `Import-Module appx -UseWindowsPowerShell\n(Get-AppPackage -Name {AppId}) -ne $null` |
| Upgrade | Same as Install |

### Fixed Properties

| Property | Value |
|----------|-------|
| InstallArgs | null |
| Configuration | null |

---

## 7. Script

Generic app with inline install/verify/upgrade scripts defined directly in app.json.

### app.json

```json
{
  "appType": "script",
  "appId": "my-custom-setup",
  "environments": "Work",
  "installScript": "iwr get.scoop.sh -OutFile $env:tmp\\install.ps1; & $env:tmp\\install.ps1",
  "verificationScript": "(Get-Command scoop -ErrorAction SilentlyContinue) -ne $null",
  "upgradeScript": null,
  "configuration": {
    "registrySettings": []
  }
}
```

### Fields

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `installScript` | string | Yes | - | PowerShell script to install |
| `verificationScript` | string | No | null | Script that returns bool |
| `upgradeScript` | string | No | null | Script to upgrade |
| `configuration` | object | No | null | Registry settings |

### Fixed Properties

| Property | Value |
|----------|-------|
| InstallArgs | null |
| PreventUpgrade | false |

---

## 8. GitRepo

Clones a git repository to a specified directory.

### app.json

```json
{
  "appType": "gitRepo",
  "appId": "git.my-project",
  "environments": "Work",
  "installArgs": "https://github.com/org/repo.git",
  "preventUpgrade": true
}
```

### Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `installArgs` | string | Yes* | Git clone URL. If null/whitespace, app is dropped |
| `preventUpgrade` | bool | No | Default false |

*If `installArgs` is null or whitespace, `ParseGitRepoApp` returns null and the app is silently excluded from the manifest.

### Runtime Properties

The `CloneRootDirectory` is set during manifest loading from `settings.git.cloneDirectory`. It ensures a trailing backslash.

The `RepoName` is derived from `InstallArgs`: strips `.git`, splits on `\` or `/`, takes the last segment.

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `mkdir {CloneRootDirectory} -Force;pushd {CloneRootDirectory};git clone {InstallArgs};popd` |
| Verify | `Test-Path {CloneRootDirectory}{RepoName}` |
| Upgrade | `pushd {CloneRootDirectory}{RepoName};git pull;popd` |

### Fixed Properties

| Property | Value |
|----------|-------|
| Configuration | null |

---

## 9. Gitconfig

Adds a git include path to the global git configuration.

### app.json

```json
{
  "appType": "gitconfig",
  "appId": "path/to/.gitconfig-custom",
  "environments": "Work"
}
```

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `git config --global --add include.path {AppId}` |
| Verify | `(git config --get-all --global include.path) -match "{AppId}"` |
| Upgrade | null (not upgradeable) |

Note: In the verification script, backslashes in the AppId are escaped (doubled) for the regex match.

### Fixed Properties

| Property | Value |
|----------|-------|
| InstallArgs | null |
| PreventUpgrade | false |
| UpgradeScript | null |
| Configuration | null |

---

## 10. NonPackageApp

App that uses a convention-based install script. No verification or upgrade.

### app.json

```json
{
  "appType": "nonPackageApp",
  "appId": "my-custom-thing",
  "environments": "Work"
}
```

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | `./{AppId}_install.ps1` |
| Verify | null |
| Upgrade | null |

### Fixed Properties

| Property | Value |
|----------|-------|
| InstallArgs | null |
| PreventUpgrade | false |
| Configuration | null |

### Note

The install script path is relative (`./{AppId}_install.ps1`), meaning it depends on the current working directory at execution time. This is fragile and may not work as expected.

---

## 11. VisualStudioExtension

Visual Studio extension installed via VSIX installer, downloaded from the Visual Studio Marketplace.

### app.json

```json
{
  "appType": "visualStudioExtension",
  "appId": "visual-studio-extension-app-id",
  "environments": "Work",
  "preventUpgrade": false,
  "downloaderArgs": {
    "Publisher": "Shanewho",
    "ExtensionName": "IHateRegions"
  }
}
```

### Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `preventUpgrade` | bool | No | Default false |
| `downloaderArgs` | object | Yes | Publisher and ExtensionName |

### Implements IDownloadApp

The `Downloader` property is hardcoded to `"VisualStudioMarketplaceDownloader"`.

### Generated Scripts

| Script | Template |
|--------|----------|
| Install | See below |
| Verify | null |
| Upgrade | Same as Install |

**Install Script:**
```powershell
$vsixInstaller = . "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -property productPath | Split-Path | % { "$_\VSIXInstaller.exe" }
$installArgs = "/quiet", "/admin", "{DownloadedFilePath}"
Start-Process $vsixInstaller $installArgs -Wait
```

### BUG NOTE

The C# source has a typo: `$vsi0xInstaller` (zero instead of 'x') in the `Start-Process` call. The PowerShell version should fix this.

### Fixed Properties

| Property | Value |
|----------|-------|
| InstallArgs | null |
| Configuration | null |

---

## 12. GitHubAsset (Internal Only)

Downloads an asset from a GitHub release. Used internally by `SelfInstaller` only. Not exposed as a manifest AppType.

### Class Name

C# class: `GitHubAssestApp` (typo - should be `GitHubAssetApp`)

### Properties

| Property | Value |
|----------|-------|
| InstallScript | `string.Empty` (no-op) |
| VerificationScript | null |
| UpgradeScript | null |
| PreventUpgrade | true |
| Configuration | null |
| InstallArgs | null |
| Downloader | `"GitHubAssetDownloader"` |

### Implements IDownloadApp

The only meaningful action is downloading the file. The install script is empty, so the AppInstaller flow will skip the install step. The downloaded file path is set by the DownloadAppInstaller.

### Not Available in Manifests

This type has no corresponding `AppType` enum value. It cannot be used in app.json files. It is created programmatically in `SelfInstaller`.

---

## App Type Summary Matrix

| App Type | InstallArgs | PreventUpgrade | Configuration | Verification | Upgrade | Download |
|----------|-------------|----------------|---------------|-------------|---------|----------|
| Winget | `--override {val}` | Settable | Settable | Yes | Yes | No |
| Scoop | ` {val}` | Settable | Settable | Yes | Yes | No |
| ScoopBucket | null | false | null | Yes (buggy) | No | No |
| PowerShell | null | false | null | Optional file | Optional file | No |
| PowerShellModule | ` {val}` | Settable | Settable | Yes | Yes | No |
| PowerShellAppPackage | null | Settable | null | Yes | Yes (=install) | Yes |
| Script | null | false | Settable | Optional field | Optional field | No |
| GitRepo | Clone URL | Settable | null | Yes | Yes | No |
| Gitconfig | null | false | null | Yes | No | No |
| NonPackageApp | null | false | null | No | No | No |
| VisualStudioExtension | null | Settable | null | No | Yes (=install) | Yes |
| GitHubAsset (internal) | null | true | null | No | No | Yes |
