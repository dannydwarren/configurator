using Configurator.Apps;
using Configurator.Installers;
using System.Threading.Tasks;

namespace Configurator.Configuration
{
    public interface IConfigureMachineCommand
    {
        Task ExecuteAsync();
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

        public async Task ExecuteAsync()
        {
            var manifest = await ManifestRepository.LoadAsync();

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
