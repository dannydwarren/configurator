function Invoke-ConfigureMachine {
    [CmdletBinding()]
    param(
        [Parameter()]
        [string[]]$Environments = @(),

        [Parameter()]
        [string]$SingleAppId
    )

    if (-not [string]::IsNullOrWhiteSpace($SingleAppId)) {
        $manifest = Import-Manifest -SingleAppId $SingleAppId
    }
    else {
        $manifest = Import-Manifest -Environments $Environments
    }

    foreach ($app in $manifest.Apps) {
        if ($app.IsDownloadApp) {
            Install-DownloadApp -App $app
        }
        else {
            Install-App -App $app
        }

        Invoke-AppConfigurator -App $app -Configure
    }
}
