using System.Threading.Tasks;
using Configurator.Apps;

namespace Configurator.Installers;

public interface IPowerShellCoreInstaller
{
    Task InstallAsync();
}

public class PowerShellCoreInstaller : IPowerShellCoreInstaller
{
    private readonly IAppInstallerForceWindowsPowerShell appInstallerForceWindowsPowerShell;

    public static readonly IApp PowerShellApp = new WingetApp
    {
        AppId = "Microsoft.PowerShell",
    };  

    public PowerShellCoreInstaller(IAppInstallerForceWindowsPowerShell appInstallerForceWindowsPowerShell)
    {
        this.appInstallerForceWindowsPowerShell = appInstallerForceWindowsPowerShell;
    }
    
    public async Task InstallAsync()
    {
        await appInstallerForceWindowsPowerShell.InstallOrUpgradeAsync(PowerShellApp);
    }
}
