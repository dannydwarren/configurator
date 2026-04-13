function Find-PowerShellCore {
    [CmdletBinding()]
    param()

    $installedVersionsKey = 'SOFTWARE\Microsoft\PowerShellCore\InstalledVersions'
    $regKey = [Microsoft.Win32.Registry]::LocalMachine.OpenSubKey($installedVersionsKey)

    if ($null -eq $regKey) {
        throw 'PowerShell Core is not installed. No versions found in registry.'
    }

    $subKeys = $regKey.GetSubKeyNames()
    $regKey.Close()

    if ($subKeys.Count -eq 0) {
        throw 'PowerShell Core is not installed. No versions found in registry.'
    }

    $versionKey = "HKEY_LOCAL_MACHINE\$installedVersionsKey\$($subKeys[0])"
    $installLocation = [Microsoft.Win32.Registry]::GetValue($versionKey, 'InstallLocation', $null)

    if ([string]::IsNullOrEmpty($installLocation)) {
        throw 'PowerShell Core is not installed. No versions found in registry.'
    }

    Join-Path $installLocation 'pwsh.exe'
}
