using System.Threading.Tasks;
using Configurator.Apps;

namespace Configurator.Installers
{
    public interface IScoopCliInstaller
    {
        Task InstallAsync();
    }

    public class ScoopCliInstaller : IScoopCliInstaller
    {
        public static IApp ScoopCliScriptApp = new ScriptApp
        {
            AppId = "ScoopCli",
            InstallScript = @"iwr get.scoop.sh -OutFile $env:tmp\scoop-install.ps1
& $env:tmp\scoop-install.ps1 -RunAsAdmin",
            VerificationScript = @"function Test-CommandExists
{
    param ($command)
    $oldPreference = $ErrorActionPreference
    $ErrorActionPreference = 'stop'
    try {if(Get-Command $command){return $true}}
    catch {return $false}
    finally {$ErrorActionPreference=$oldPreference}
}

Test-CommandExists scoop"
        };

        private readonly IAppInstaller appInstaller;

        public ScoopCliInstaller(IAppInstaller appInstaller)
        {
            this.appInstaller = appInstaller;
        }

        public async Task InstallAsync()
        {
            await appInstaller.InstallOrUpgradeAsync(ScoopCliScriptApp);
        }
    }
}
