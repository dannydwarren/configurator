function Write-ConfiguratorLog {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Message,

        [Parameter()]
        [ValidateSet('Debug', 'Verbose', 'Info', 'Warn', 'Error', 'Progress', 'Result')]
        [string]$Level = 'Info'
    )

    $labelMap = @{
        Debug    = 'DEBUG'
        Verbose  = 'INFOV'
        Info     = 'INFO '
        Warn     = 'WARN '
        Error    = 'ERROR'
        Progress = 'PRGRS'
        Result   = 'RESLT'
    }

    $colorMap = @{
        Debug    = 'Gray'
        Verbose  = 'White'
        Info     = 'White'
        Warn     = 'Yellow'
        Error    = 'Red'
        Progress = 'Blue'
        Result   = 'Green'
    }

    $timestamp = Get-Date -Format 'o'
    $label = $labelMap[$Level]
    $color = $colorMap[$Level]

    $logMessage = "[$timestamp] [$label] $Message"
    Write-Host $logMessage -ForegroundColor $color
}
