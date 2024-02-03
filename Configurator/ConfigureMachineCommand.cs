using Configurator.Apps;
using Configurator.Installers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configurator
{
    public interface IConfigureMachineCommand
    {
        Task ExecuteAsync(List<string> environments, string singleAppId);
    }

    public class ConfigureMachineCommand : IConfigureMachineCommand
    {
        public IManifestRepository ManifestRepository { get; }
        public IAppInstaller AppInstaller { get; }
        public IDownloadAppInstaller DownloadAppInstaller { get; }
        public IAppConfigurator AppConfigurator { get; }

        public ConfigureMachineCommand(IManifestRepository manifestRepository,
            IAppInstaller appInstaller,
            IDownloadAppInstaller downloadAppInstaller,
            IAppConfigurator appConfigurator)
        {
            ManifestRepository = manifestRepository;
            AppInstaller = appInstaller;
            DownloadAppInstaller = downloadAppInstaller;
            AppConfigurator = appConfigurator;
        }

        public async Task ExecuteAsync(List<string> environments, string singleAppId)
        {
            var installSingleApp = !string.IsNullOrEmpty(singleAppId);
            if (installSingleApp)
            {
                await ConfigureSingleAppAsync(singleAppId);
            }
            else
            {
                await ConfigureEnvironmentAppsAsync(environments);
            }
        }

        private async Task ConfigureSingleAppAsync(string appId)
        {
            var app = await ManifestRepository.LoadAppAsync(appId);

            await ConfigureAppAsync(app);
        }

        private async Task ConfigureEnvironmentAppsAsync(List<string> environments)
        {
            var manifest = await ManifestRepository.LoadAsync(environments);

            foreach (var app in manifest.Apps)
            {
                await ConfigureAppAsync(app);
            }
        }

        private async Task ConfigureAppAsync(IApp app)
        {
            if (app is IDownloadApp downloadApp)
            {
                await DownloadAppInstaller.InstallAsync(downloadApp);
            }
            else
            {
                await AppInstaller.InstallOrUpgradeAsync(app);
            }

            AppConfigurator.Configure(app);
        }
    }
}
