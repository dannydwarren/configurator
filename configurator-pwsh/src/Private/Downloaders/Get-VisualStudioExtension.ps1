function Get-VisualStudioExtension {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [PSCustomObject]$DownloaderArgs
    )

    $publisher = $DownloaderArgs.Publisher
    $extensionName = $DownloaderArgs.ExtensionName

    $pageUrl = "https://marketplace.visualstudio.com/items?itemName=$publisher.$extensionName"
    $pageResponse = Invoke-WebRequest -Uri $pageUrl -UseBasicParsing
    $pageHtml = $pageResponse.Content

    $versionedUrlPattern = "/_apis/public/gallery/publishers/$publisher/vsextensions/$extensionName/(\d+\.?)+/vspackage"
    $match = [regex]::Match($pageHtml, $versionedUrlPattern)
    $downloadUrl = "https://marketplace.visualstudio.com$($match.Value)"

    $fileName = "$publisher.$extensionName.vsix"

    $settings = Import-Settings
    Get-Download -Url $downloadUrl -FileName $fileName -DownloadsDirectory $settings.downloadsDirectory
}
