using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Downloaders;
using Configurator.PowerShell;
using Configurator.Utilities;

namespace Configurator.Installers
{
    public interface IDownloadAppInstaller
    {
        Task InstallOrUpgradeAsync(IDownloadApp app);
    }

    public class DownloadAppInstaller : IDownloadAppInstaller
    {
        private readonly IConsoleLogger consoleLogger;
        private readonly IAppInstaller appInstaller;
        private readonly IDownloaderFactory downloaderFactory;

        public DownloadAppInstaller(IConsoleLogger consoleLogger,
            IAppInstaller appInstaller,
            IDownloaderFactory downloaderFactory)
        {
            this.consoleLogger = consoleLogger;
            this.appInstaller = appInstaller;
            this.downloaderFactory = downloaderFactory;
        }

        public async Task InstallOrUpgradeAsync(IDownloadApp app)
        {
            consoleLogger.Info($"Downloading '{app.AppId}'");

            IDownloader downloader = downloaderFactory.GetDownloader(app.Downloader);

            app.DownloadedFilePath = await downloader.DownloadAsync(app.DownloaderArgs.ToString()!);

            consoleLogger.Result($"Downloaded '{app.AppId}'");

            await appInstaller.InstallOrUpgradeAsync(app);
        }
    }
}
