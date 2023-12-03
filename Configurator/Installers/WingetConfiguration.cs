using System;
using System.Threading.Tasks;
using Configurator.PowerShell;

namespace Configurator.Installers
{
    public interface IWingetConfiguration
    {
        Task UpgradeAsync();
        Task AcceptSourceAgreementsAsync();
    }

    public class WingetConfiguration : IWingetConfiguration
    {
        private readonly IPowerShell powerShell;

        public WingetConfiguration(IPowerShell powerShell)
        {
            this.powerShell = powerShell;
        }
        public async Task UpgradeAsync()
        {
            await powerShell.ExecuteWindowsAsync("Add-AppxPackage https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle -ForceTargetApplicationShutdown");
            await powerShell.ExecuteWindowsAsync("Add-AppxPackage https://cdn.winget.microsoft.com/cache/source.msix");
        }

        public async Task AcceptSourceAgreementsAsync()
        {
            await powerShell.ExecuteWindowsAsync("winget list winget --accept-source-agreements");
        }
    }
}
