﻿using Configurator.Git;
using Configurator.PowerShell;
using Configurator.Scoop;
using Configurator.Utilities;
using System.Threading.Tasks;

namespace Configurator
{
    public interface IMachineConfigurator
    {
        Task ExecuteAsync();
    }

    public class MachineConfigurator : IMachineConfigurator
    {
        private readonly IArguments arguments;
        private readonly IPowerShellConfiguration powerShellConfiguration;
        private readonly IScoopInstaller scoopInstaller;
        private readonly IScoopAppRepository scoopList;
        private readonly IGitConfiguration gitConfiguration;

        public MachineConfigurator(IArguments arguments,
            IPowerShellConfiguration powerShellConfiguration,
            IScoopInstaller scoopInstaller,
            IScoopAppRepository scoopList,
            IGitConfiguration gitConfiguration)
        {
            this.arguments = arguments;
            this.powerShellConfiguration = powerShellConfiguration;
            this.scoopInstaller = scoopInstaller;
            this.scoopList = scoopList;
            this.gitConfiguration = gitConfiguration;
        }

        public async Task ExecuteAsync()
        {
            await powerShellConfiguration.SetExecutionPolicyAsync();

            await InstallScoopAppsAsync();

            await gitConfiguration.IncludeCustomGitconfigAsync(arguments.GitconfigPath);
        }

        private async Task InstallScoopAppsAsync()
        {
            var scoopAppsToInstall = await scoopList.LoadAsync();

            foreach (var scoopApp in scoopAppsToInstall)
            {
                await scoopInstaller.InstallAsync(scoopApp.AppId);
            }
        }
    }
}
