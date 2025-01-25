using System.IO;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Configuration;
using Configurator.PowerShell;
using Configurator.Utilities;
using Configurator.Windows;

namespace Configurator.Installers
{
    public interface IAppConfigurator
    {
        void Configure(IApp app);
        Task Backup(IApp app);
    }

    public class AppConfigurator : IAppConfigurator
    {
        private readonly IRegistryRepository registryRepository;
        private readonly ISettingsRepository settingsRepository;
        private readonly IFileSystem fileSystem;
        private readonly IPowerShell powerShell;

        public AppConfigurator(
            IRegistryRepository registryRepository,
            ISettingsRepository settingsRepository,
            IFileSystem fileSystem,
            IPowerShell powerShell)
        {
            this.registryRepository = registryRepository;
            this.settingsRepository = settingsRepository;
            this.fileSystem = fileSystem;
            this.powerShell = powerShell;
        }

        public void Configure(IApp app)
        {
            if (app.Configuration == null)
                return;

            app.Configuration.RegistrySettings.ForEach(setting =>
            {
                registryRepository.SetValue(setting.KeyName, setting.ValueName, setting.ValueData);
            });
        }

        public async Task Backup(IApp app)
        {
            var isInstalled = !string.IsNullOrEmpty(app.VerificationScript)
                              && await powerShell.ExecuteAsync<bool>(app.VerificationScript);
            
            if (!isInstalled)
            {
                return;
            }

            var settings = await settingsRepository.LoadSettingsAsync();
            var backupFilePath = Path.Join(settings.Manifest.Directory, $@"apps\{app.AppId}\backup.ps1");
            if (fileSystem.Exists(backupFilePath))
            {
                await powerShell.ExecuteAsync(backupFilePath);
            }
        }
    }
}
