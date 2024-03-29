﻿using System.Threading.Tasks;
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
        private readonly IWingetConfiguration wingetConfiguration;
        private readonly IScoopCliInstaller scoopCliInstaller;
        private readonly IGitInstaller gitInstaller;
        private readonly IManifestRepoInstaller manifestRepoInstaller;

        public SystemInitializer(IPowerShellConfiguration powerShellConfiguration,
            IPowerShellCoreInstaller powerShellCoreInstaller,
            IWingetConfiguration wingetConfiguration,
            IScoopCliInstaller scoopCliInstaller,
            IGitInstaller gitInstaller,
            IManifestRepoInstaller manifestRepoInstaller)
        {
            this.powerShellConfiguration = powerShellConfiguration;
            this.powerShellCoreInstaller = powerShellCoreInstaller;
            this.wingetConfiguration = wingetConfiguration;
            this.scoopCliInstaller = scoopCliInstaller;
            this.gitInstaller = gitInstaller;
            this.manifestRepoInstaller = manifestRepoInstaller;
        }

        public async Task InitializeAsync()
        {
            await powerShellConfiguration.SetWindowsPowerShellExecutionPolicyAsync();
            await wingetConfiguration.UpgradeAsync();
            await wingetConfiguration.AcceptSourceAgreementsAsync();
            await powerShellCoreInstaller.InstallAsync();
            await powerShellConfiguration.SetPowerShellCoreExecutionPolicyAsync();
            await scoopCliInstaller.InstallAsync();
            await gitInstaller.InstallAsync();
            await manifestRepoInstaller.InstallAsync();
        }
    }
}
