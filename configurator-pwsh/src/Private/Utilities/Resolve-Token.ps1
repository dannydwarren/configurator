function Resolve-Token {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Value
    )

    $pattern = '(\{\{(.*?)\}\})+'
    $result = $Value

    [regex]::Matches($Value, $pattern) | ForEach-Object {
        $raw = $_.Value
        $inner = $raw.TrimStart('{').TrimEnd('}')
        $parts = $inner.Split(':', [System.StringSplitOptions]::RemoveEmptyEntries)
        $source = $parts[0]
        $name = $parts[1]

        $replacement = ''
        if ($source -eq 'env') {
            $envValue = [System.Environment]::GetEnvironmentVariable($name)
            if ($null -ne $envValue) {
                $replacement = $envValue
            }
        }

        $result = $result.Replace($raw, $replacement)
    }

    $result
}
