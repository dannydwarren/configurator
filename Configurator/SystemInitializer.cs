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
        private readonly IPowerShellInstaller powerShellInstaller;
        private readonly IWingetCliInstaller wingetCliInstaller;
        private readonly IScoopCliInstaller scoopCliInstaller;
        private readonly IGitInstaller gitInstaller;

        public SystemInitializer(IPowerShellConfiguration powerShellConfiguration,
            IPowerShellInstaller powerShellInstaller,
            IWingetCliInstaller wingetCliInstaller,
            IScoopCliInstaller scoopCliInstaller,
            IGitInstaller gitInstaller)
        {
            this.powerShellConfiguration = powerShellConfiguration;
            this.powerShellInstaller = powerShellInstaller;
            this.wingetCliInstaller = wingetCliInstaller;
            this.scoopCliInstaller = scoopCliInstaller;
            this.gitInstaller = gitInstaller;
        }

        public async Task InitializeAsync()
        {
            await powerShellConfiguration.SetWindowsPowerShellExecutionPolicyAsync();
            await powerShellInstaller.InstallAsync();
            await powerShellConfiguration.SetPowerShellCoreExecutionPolicyAsync();
            await wingetCliInstaller.InstallAsync();
            await scoopCliInstaller.InstallAsync();
            await gitInstaller.InstallAsync();
        }
    }
}
