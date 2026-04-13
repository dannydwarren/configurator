function Set-PowerShellPolicy {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Core', 'Windows')]
        [string]$Edition
    )

    $ErrorActionPreference = 'Stop'

    $setPolicyScript = 'Set-ExecutionPolicy RemoteSigned -Force'
    $getPolicyScript = 'Get-ExecutionPolicy'
    $getVersionScript = '$PSVersionTable.PSVersion.ToString()'

    if ($Edition -eq 'Core') {
        Invoke-PowerShellScript -Script $setPolicyScript -RunAsAdmin
        $policyResult = Invoke-PowerShellScript -Script $getPolicyScript -ResultType ([string])
        Write-ConfiguratorLog -Message "PowerShell Core - Execution Policy: $policyResult" -Level Result

        $versionResult = Invoke-PowerShellScript -Script $getVersionScript -ResultType ([string])
        Write-ConfiguratorLog -Message "PowerShell Core - Version: $versionResult" -Level Debug
    }
    else {
        Invoke-WindowsPowerShellScript -Script $setPolicyScript -RunAsAdmin
        $policyResult = Invoke-WindowsPowerShellScript -Script $getPolicyScript -ResultType ([string])
        Write-ConfiguratorLog -Message "Windows PowerShell - Execution Policy: $policyResult" -Level Result

        $versionResult = Invoke-WindowsPowerShellScript -Script $getVersionScript -ResultType ([string])
        Write-ConfiguratorLog -Message "Windows PowerShell - Version: $versionResult" -Level Debug
    }
}
