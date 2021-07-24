﻿using System.Collections.Generic;
using Configurator.Git;
using Configurator.PowerShell;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Installers;

namespace Configurator
{
    public interface IMachineConfigurator
    {
        Task ExecuteAsync();
    }

    public class MachineConfigurator : IMachineConfigurator
    {
        private readonly IPowerShellConfiguration powerShellConfiguration;
        private readonly IAppsRepository appsRepository;
        private readonly IGitConfiguration gitConfiguration;
        private readonly IGitconfigRepository gitconfigRepository;
        private readonly IAppInstaller appInstaller;
        private readonly IDownloadInstaller downloadInstaller;

        public MachineConfigurator(IPowerShellConfiguration powerShellConfiguration,
            IAppsRepository appsRepository,
            IGitConfiguration gitConfiguration,
            IGitconfigRepository gitconfigRepository,
            IAppInstaller appInstaller,
            IDownloadInstaller downloadInstaller)
        {
            this.powerShellConfiguration = powerShellConfiguration;
            this.appsRepository = appsRepository;
            this.gitConfiguration = gitConfiguration;
            this.gitconfigRepository = gitconfigRepository;
            this.appInstaller = appInstaller;
            this.downloadInstaller = downloadInstaller;
        }

        public async Task ExecuteAsync()
        {
            await powerShellConfiguration.SetExecutionPolicyAsync();

            var apps = await appsRepository.LoadAsync();

            await InstallPowerShellAppPackages(apps.PowerShellAppPackages);
            await IncludeCustomGitconfigsAsync();
            await InstallWingetAppsAsync(apps.WingetApps);
            await InstallScoopAppsAsync(apps.ScoopApps);
        }

        private async Task InstallPowerShellAppPackages(List<PowerShellAppPackage> powerShellAppPackages)
        {
            foreach (var appPackage in powerShellAppPackages)
            {
                await downloadInstaller.InstallAsync(appPackage);
            }
        }

        private async Task IncludeCustomGitconfigsAsync()
        {
            var gitconfigs = await gitconfigRepository.LoadAsync();

            foreach (var gitconfig in gitconfigs)
            {
                await gitConfiguration.IncludeGitconfigAsync(gitconfig.Path);
            }
        }

        private async Task InstallWingetAppsAsync(List<WingetApp> wingetApps)
        {
            foreach (var wingetApp in wingetApps)
            {
                await appInstaller.InstallAsync(wingetApp);
            }
        }

        private async Task InstallScoopAppsAsync(List<ScoopApp> scoopApps)
        {
            foreach (var scoopApp in scoopApps)
            {
                await appInstaller.InstallAsync(scoopApp);
            }
        }
    }
}
