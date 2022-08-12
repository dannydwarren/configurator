using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.PowerShell;
using Configurator.Utilities;

namespace Configurator
{
    public interface IInitializeCommand
    {
        Task ExecuteAsync();
    }

    public class InitializeCommand : IInitializeCommand
    {
        private readonly ISystemInitializer systemInitializer;
        private readonly ISettingsRepository settingsRepository;
        private readonly IPowerShell_Obsolete powerShell;
        private readonly IFileSystem fileSystem;

        public InitializeCommand(ISystemInitializer systemInitializer,
            ISettingsRepository settingsRepository,
            IPowerShell_Obsolete powerShell,
            IFileSystem fileSystem)
        {
            this.systemInitializer = systemInitializer;
            this.settingsRepository = settingsRepository;
            this.powerShell = powerShell;
            this.fileSystem = fileSystem;
        }
        
        public async Task ExecuteAsync()
        {
            await systemInitializer.InitializeAsync();

            var settings = await SetManifestDirectoryAsync();

            await InitializeManifestDirectory(settings);

            await InitializeManifestFile(settings);
        }

        private async Task<Settings> SetManifestDirectoryAsync()
        {
            var settings = await settingsRepository.LoadSettingsAsync();

            if (settings.Manifest.Repo == null)
            {
                throw new Exception("The 'manifest.repo' setting must be set before invoking initialize.");
            }

            var repoName = settings.Manifest.Repo.Segments.Last().Replace(".git", "");
            var manifestDirectory = Path.Combine(settings.Git.CloneDirectory.AbsolutePath, repoName);

            if (settings.Manifest.Directory != manifestDirectory)
            {
                settings.Manifest.Directory = manifestDirectory;
                await settingsRepository.SaveAsync(settings);
            }

            return settings;
        }

        private async Task InitializeManifestDirectory(Settings settings)
        {
            if (!fileSystem.Exists(settings.Manifest.Directory!))
            {
                await powerShell.ExecuteAsync($@"
Push-Location {settings.Git.CloneDirectory.AbsolutePath}
git clone {settings.Manifest.Repo!.AbsoluteUri}
Pop-Location");
            }
        }

        private async Task InitializeManifestFile(Settings settings)
        {
            var fullyQualifiedManifestFilePath = Path.Combine(settings.Manifest.Directory!, settings.Manifest.FileName);

            if (!fileSystem.Exists(fullyQualifiedManifestFilePath))
            {
                await fileSystem.WriteAllTextAsync(fullyQualifiedManifestFilePath, "{ }");

                await powerShell.ExecuteAsync($@"
Push-Location {settings.Manifest.Directory}
git add .
git commit -m '[Configurator] Create manifest file'
git push
Pop-Location");
            }
        }
    }
}
