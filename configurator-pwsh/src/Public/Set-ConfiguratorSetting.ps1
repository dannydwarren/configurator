function Set-ConfiguratorSetting {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Name,

        [Parameter(Mandatory)]
        [string]$Value
    )

    $ErrorActionPreference = 'Stop'

    $settings = Import-Settings

    if (-not (Find-AndSetSetting -SettingPath $Name -SettingValue $Value -ParentNode $settings -ParentPrefix '')) {
        throw "$Name is not a recognized setting name."
    }

    Export-Settings -Settings $settings
}

function Find-AndSetSetting {
    param(
        [string]$SettingPath,
        [string]$SettingValue,
        [PSCustomObject]$ParentNode,
        [string]$ParentPrefix
    )

    $uriSettings = @('downloadsdirectory', 'manifest.repo', 'git.clonedirectory')

    foreach ($prop in $ParentNode.PSObject.Properties) {
        $path = if ($ParentPrefix -eq '') { $prop.Name.ToLower() } else { "$ParentPrefix.$($prop.Name.ToLower())" }
        $val = $prop.Value

        if ($null -ne $val -and $val -is [PSCustomObject]) {
            if (Find-AndSetSetting -SettingPath $SettingPath -SettingValue $SettingValue -ParentNode $val -ParentPrefix $path) {
                return $true
            }
        }
        else {
            if ($path -eq $SettingPath.ToLower()) {
                if ($uriSettings -contains $path) {
                    $ParentNode.$($prop.Name) = [uri]$SettingValue
                }
                else {
                    $ParentNode.$($prop.Name) = $SettingValue
                }
                return $true
            }
        }
    }

    return $false
}
