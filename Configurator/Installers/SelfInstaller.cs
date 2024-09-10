using Configurator.Apps;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Configurator.Downloaders;
using Configurator.Utilities;

namespace Configurator.Installers
{
    public interface ISelfInstaller
    {
        Task InstallAsync();
    }

    public class SelfInstaller : ISelfInstaller
    {
        private static readonly string ConfiguratorInstallationDirectory = "c:\\Configurator";
        private readonly IDownloadAppInstaller appInstaller;
        private readonly IFileSystem fileSystem;
        private static readonly GitHubAssestApp configuratorApp = new()
        {
            AppId = "Configurator",
        };

        public SelfInstaller(IDownloadAppInstaller appInstaller, IFileSystem fileSystem)
        {
            this.appInstaller = appInstaller;
            this.fileSystem = fileSystem;
        }


        public async Task InstallAsync()
        {
            if (fileSystem.Exists(ConfiguratorInstallationDirectory))
            {
                return;
            }

            await SetConfiguratorAppDownloaderArgs();

            await appInstaller.InstallOrUpgradeAsync(configuratorApp);

            MoveToInstallationDirectory();
        }

        private void MoveToInstallationDirectory()
        {
            fileSystem.CreateDirectory(ConfiguratorInstallationDirectory);
            fileSystem.MoveFile(configuratorApp.DownloadedFilePath, Path.Combine(ConfiguratorInstallationDirectory, "Configurator.exe"));
        }

        private static async Task SetConfiguratorAppDownloaderArgs()
        {
            var buffer = Encoding.UTF8.GetBytes($@"{{ 
                ""{nameof(GitHubAssetDownloaderArgs.User)}"": ""dannydwarren"",
                ""{nameof(GitHubAssetDownloaderArgs.Repo)}"": ""configurator"",
                ""{nameof(GitHubAssetDownloaderArgs.Extension)}"": "".exe""
            }}");
            var memoryStream = new MemoryStream(buffer);
            var downloaderArgsDoc = await JsonDocument.ParseAsync(memoryStream);
            configuratorApp.DownloaderArgs = downloaderArgsDoc.RootElement;
        }
    }
}
