function Get-Download {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string]$Url,

        [Parameter(Mandatory)]
        [string]$FileName,

        [Parameter(Mandatory)]
        [string]$DownloadsDirectory
    )

    # TODO: Implement - HTTP GET url, check status 200, write to downloads dir, return full path
    throw "Not implemented"
}
