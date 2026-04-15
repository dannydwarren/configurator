function Set-RegistryValue {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$KeyName,

        [Parameter(Mandatory)]
        [string]$ValueName,

        [Parameter(Mandatory)]
        $ValueData
    )

    if ($ValueData -is [uint32] -or $ValueData -is [int]) {
        $registryValueKind = [Microsoft.Win32.RegistryValueKind]::DWord
        if ($ValueData -is [uint32]) {
            $ValueData = [int][uint32]$ValueData
        }
    }
    elseif ($ValueData -is [string]) {
        $registryValueKind = [Microsoft.Win32.RegistryValueKind]::String
    }
    else {
        throw "RegistrySetting.ValueData only supports types: string and uint32"
    }

    try {
        [Microsoft.Win32.Registry]::SetValue($KeyName, $ValueName, $ValueData, $registryValueKind)
    }
    catch {
        Write-ConfiguratorLog -Message "Error setting value in registry >> keyName: $KeyName; valueName: $ValueName; RegistryValueKind: $registryValueKind`n$_" -Level Error
        throw
    }
}
