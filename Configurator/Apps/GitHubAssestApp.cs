using Configurator.Downloaders;
using System.Text.Json;

namespace Configurator.Apps
{
    public class GitHubAssestApp : IDownloadApp
    {
        public string AppId {  get; set; }

        public string? InstallArgs => null;

        public bool PreventUpgrade => true;

        public string InstallScript => string.Empty;

        public string? VerificationScript => null;

        public string? UpgradeScript => null;

        public AppConfiguration? Configuration => null;

        public string Downloader => nameof(GitHubAssetDownloader);

        public string DownloadedFilePath { get; set; }

        public JsonElement DownloaderArgs { get; set; }
    }
}
