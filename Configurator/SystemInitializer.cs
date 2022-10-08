using System.Threading.Tasks;
using Configurator.Installers;
using Configurator.PowerShell;

namespace Configurator
{
    public interface ISystemInitializer
    {
        Task InitializeAsync();
    }

    public class SystemInitializer : ISystemInitializer
    {
        private readonly IPowerShellConfiguration powerShellConfiguration;
        private readonly IPowerShellCoreInstaller powerShellCoreInstaller;
        private readonly IWingetCliInstaller wingetCliInstaller;
        private readonly IScoopCliInstaller scoopCliInstaller;
        private readonly IGitInstaller gitInstaller;

        public SystemInitializer(IPowerShellConfiguration powerShellConfiguration,
            IPowerShellCoreInstaller powerShellCoreInstaller,
            IWingetCliInstaller wingetCliInstaller,
            IScoopCliInstaller scoopCliInstaller,
            IGitInstaller gitInstaller)
        {
            this.powerShellConfiguration = powerShellConfiguration;
            this.powerShellCoreInstaller = powerShellCoreInstaller;
            this.wingetCliInstaller = wingetCliInstaller;
            this.scoopCliInstaller = scoopCliInstaller;
            this.gitInstaller = gitInstaller;
        }

        public async Task InitializeAsync()
        {
            await powerShellConfiguration.SetWindowsPowerShellExecutionPolicyAsync();
            await wingetCliInstaller.InstallAsync();
            await powerShellCoreInstaller.InstallAsync();
            await powerShellConfiguration.SetPowerShellCoreExecutionPolicyAsync();
            await scoopCliInstaller.InstallAsync();
            await gitInstaller.InstallAsync();
        }
    }
}
