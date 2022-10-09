using System.Threading.Tasks;
using Configurator.PowerShell;

namespace Configurator.Installers
{
    public interface IWingetConfiguration
    {
        Task AcceptSourceAgreementsAsync();
    }

    public class WingetConfiguration : IWingetConfiguration
    {
        private readonly IPowerShell powerShell;

        public WingetConfiguration(IPowerShell powerShell)
        {
            this.powerShell = powerShell;
        }

        public async Task AcceptSourceAgreementsAsync()
        {
            await powerShell.ExecuteWindowsAsync("winget list winget --accept-source-agreements");
        }
    }
}
