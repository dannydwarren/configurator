function Get-ConfiguratorSettings {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    $settings = Import-Settings

    $rows = Get-SettingRows -Node $settings -Prefix ''

    $rows | Format-Table -Property Name, Value, Type -AutoSize
}

function Get-SettingRows {
    param(
        [PSCustomObject]$Node,
        [string]$Prefix
    )

    $uriSettings = @('downloadsdirectory', 'manifest.repo', 'git.clonedirectory')

    $rows = @()

    foreach ($prop in $Node.PSObject.Properties) {
        $path = if ($Prefix -eq '') { $prop.Name.ToLower() } else { "$Prefix.$($prop.Name.ToLower())" }
        $val = $prop.Value

        if ($null -ne $val -and $val -is [PSCustomObject]) {
            $rows += Get-SettingRows -Node $val -Prefix $path
        }
        else {
            $typeName = if ($uriSettings -contains $path) {
                'Uri'
            }
            elseif ($val -is [uri]) {
                'Uri'
            }
            else {
                'String'
            }

            $displayValue = if ($null -eq $val) { '' } else { $val.ToString() }

            $rows += [PSCustomObject]@{
                Name  = $path
                Value = $displayValue
                Type  = $typeName
            }
        }
    }

    $rows
}
