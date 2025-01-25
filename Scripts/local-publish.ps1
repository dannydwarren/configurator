
$configuratorScriptsPath = (Split-Path -parent $MyInvocation.MyCommand.Definition)
$configuratorDevDeploymentPath = "c:\Configurator"

if (-not (Test-Path $configuratorDevDeploymentPath)){
    mkdir $configuratorDevDeploymentPath -Force
}

$env:Path += ";$configuratorDevDeploymentPath"

function configurator-publish() {
    $projectPath = "$configuratorScriptsPath\..\Configurator\Configurator.csproj"
    Write-Host "Configurator Project: $projectPath"
    dotnet publish $projectPath /p:Version="0.0.0-dev" --configuration Release --output $configuratorDevDeploymentPath
}
