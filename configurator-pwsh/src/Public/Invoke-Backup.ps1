function Invoke-Backup {
    [CmdletBinding()]
    param()

    $manifest = Import-Manifest -Environments @()

    foreach ($app in $manifest.Apps) {
        Invoke-AppConfigurator -App $app -Backup
    }
}
