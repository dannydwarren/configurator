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

    $filePath = Join-Path $DownloadsDirectory $FileName

    $response = Invoke-WebRequest -Uri $Url -OutFile $filePath -PassThru

    if ($response.StatusCode -ne 200) {
        throw "Failed with status code $($response.StatusCode) to download $FileName"
    }

    $filePath
}
