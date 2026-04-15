function Get-RegistryValue {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$KeyName,

        [Parameter(Mandatory)]
        [string]$ValueName
    )

    $value = [Microsoft.Win32.Registry]::GetValue($KeyName, $ValueName, '')

    if ($value -is [int]) {
        $value = [System.BitConverter]::ToUInt32([System.BitConverter]::GetBytes($value), 0)
    }

    if ($value -is [long]) {
        $value = [System.BitConverter]::ToUInt64([System.BitConverter]::GetBytes($value), 0)
    }

    $value.ToString()
}
