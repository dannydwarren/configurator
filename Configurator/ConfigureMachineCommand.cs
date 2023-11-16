using Configurator.Apps;
using Configurator.Installers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configurator
{
    public interface IConfigureMachineCommand
    {
        Task ExecuteAsync(List<string> environments);
    }

    public class ConfigureMachineCommand : IConfigureMachineCommand
    {
        public IManifestRepository_V2 ManifestRepository { get; }
        public IAppInstaller AppInstaller { get; }
        public IDownloadAppInstaller DownloadAppInstaller { get; }
        public IAppConfigurator AppConfigurator { get; }

        public ConfigureMachineCommand(IManifestRepository_V2 manifestRepository,
            IAppInstaller appInstaller,
            IDownloadAppInstaller downloadAppInstaller,
            IAppConfigurator appConfigurator)
        {
            ManifestRepository = manifestRepository;
            AppInstaller = appInstaller;
            DownloadAppInstaller = downloadAppInstaller;
            AppConfigurator = appConfigurator;
        }

        public async Task ExecuteAsync(List<string> environments)
        {
            var manifest = await ManifestRepository.LoadAsync(environments);

            foreach (var app in manifest.Apps)
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
}
