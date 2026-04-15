function Get-DesktopEntries {
    [CmdletBinding()]
    param()

    $desktopPaths = @(
        [System.Environment]::GetFolderPath('Desktop')
        [System.Environment]::GetFolderPath('CommonDesktopDirectory')
    )

    $entries = @()
    foreach ($path in $desktopPaths) {
        if (Test-Path $path) {
            $entries += Get-ChildItem -Path $path -Force | Select-Object -ExpandProperty FullName
        }
    }

    $entries | Select-Object -Unique
}

function Remove-DesktopShortcuts {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]]$BeforeEntries,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]]$AfterEntries
    )

    $newEntries = $AfterEntries | Where-Object { $_ -notin $BeforeEntries }

    foreach ($entry in $newEntries) {
        if (Test-Path $entry -PathType Leaf) {
            Remove-Item -Path $entry -Force
        }
        elseif (Test-Path $entry -PathType Container) {
            Remove-Item -Path $entry -Force
        }
    }
}
