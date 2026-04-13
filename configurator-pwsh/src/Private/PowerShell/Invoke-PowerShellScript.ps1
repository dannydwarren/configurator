function Invoke-PowerShellScript {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Script,

        [Parameter()]
        [switch]$RunAsAdmin,

        [Parameter()]
        [type]$ResultType
    )

    $myDocumentsPath = [System.Environment]::GetFolderPath('MyDocuments')
    $profilePath = Join-Path $myDocumentsPath 'PowerShell' 'Microsoft.PowerShell_profile.ps1'

    $environmentReadyScript = @"
`$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path","User")

if (`$profile -eq `$null -or `$profile -eq '') {
  `$global:profile = "$profilePath"
}

$Script
"@

    $scriptFile = New-ScriptFile -Script $environmentReadyScript
    $executable = Find-PowerShellCore

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $executable
    $startInfo.Arguments = "-File $scriptFile"
    $startInfo.UseShellExecute = [bool]$RunAsAdmin
    $startInfo.RedirectStandardOutput = -not $RunAsAdmin
    $startInfo.RedirectStandardError = -not $RunAsAdmin
    $startInfo.Verb = if ($RunAsAdmin) { 'runas' } else { 'open' }

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    $process.Start() | Out-Null

    $lastOutput = $null
    $allOutput = @()
    $errors = @()

    if (-not $RunAsAdmin) {
        while (-not $process.StandardOutput.EndOfStream) {
            $line = $process.StandardOutput.ReadLine()
            if ($null -ne $line) {
                $lastOutput = $line
                $allOutput += $line
                Write-ConfiguratorLog -Message $line -Level Debug
            }
        }

        while (-not $process.StandardError.EndOfStream) {
            $dirtyError = $process.StandardError.ReadLine()
            if ($null -ne $dirtyError) {
                $cleanError = $dirtyError -replace "$([char]27)\[91m", '' -replace "$([char]27)\[31;1m", '' -replace "$([char]27)\[0m", ''
                $errors += $cleanError
                Write-ConfiguratorLog -Message $cleanError -Level Debug
            }
        }
    }

    $process.WaitForExit()
    $exitCode = $process.ExitCode
    $process.Dispose()

    foreach ($err in $errors) {
        if (-not [string]::IsNullOrWhiteSpace($err)) {
            Write-ConfiguratorLog -Message $err -Level Error
        }
    }

    if ($exitCode -ne 0) {
        throw "Script failed to complete with exit code $exitCode"
    }

    if ($null -ne $ResultType) {
        if ($null -eq $lastOutput) {
            return $null
        }

        if ($ResultType -eq [string]) {
            return $lastOutput
        }
        elseif ($ResultType -eq [bool]) {
            return [bool]::Parse($lastOutput)
        }
        else {
            throw "PowerShell result type of '$($ResultType.FullName)' is not yet supported"
        }
    }
}
