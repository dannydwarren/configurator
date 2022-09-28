using System.Threading.Tasks;
using Configurator.Apps;

namespace Configurator.Installers;

public interface IPowerShellInstaller
{
    Task InstallAsync();
}

public class PowerShellInstaller : IPowerShellInstaller
{
    private readonly IAppInstallerForceWindowsPowerShell appInstallerForceWindowsPowerShell;

    public static readonly IApp PowerShellApp = new WingetApp
    {
        AppId = "Microsoft.PowerShell",
    };  

    public PowerShellInstaller(IAppInstallerForceWindowsPowerShell appInstallerForceWindowsPowerShell)
    {
        this.appInstallerForceWindowsPowerShell = appInstallerForceWindowsPowerShell;
    }
    
    public async Task InstallAsync()
    {
        await appInstallerForceWindowsPowerShell.InstallOrUpgradeAsync(PowerShellApp);
    }
}
