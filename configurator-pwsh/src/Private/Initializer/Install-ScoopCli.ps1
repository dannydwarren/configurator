function Install-ScoopCli {
    [CmdletBinding()]
    param()

    $ErrorActionPreference = 'Stop'

    $app = New-ScriptApp -RawApp ([PSCustomObject]@{
        appId              = 'ScoopCli'
        environments       = ''
        installScript      = @"
iwr get.scoop.sh -OutFile `$env:tmp\scoop-install.ps1
& `$env:tmp\scoop-install.ps1 -RunAsAdmin
"@
        verificationScript = @"
function Test-CommandExists
{
    param (`$command)
    `$oldPreference = `$ErrorActionPreference
    `$ErrorActionPreference = 'stop'
    try {if(Get-Command `$command){return `$true}}
    catch {return `$false}
    finally {`$ErrorActionPreference=`$oldPreference}
}

Test-CommandExists scoop
"@
        upgradeScript      = $null
    })

    Install-App -App $app
}
