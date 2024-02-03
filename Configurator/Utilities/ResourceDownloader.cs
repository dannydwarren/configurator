using System;
using System.Threading.Tasks;
using Configurator.Configuration;
using Emmersion.Http;

namespace Configurator.Utilities
{
    public interface IResourceDownloader
    {
        Task<string> DownloadAsync(string fileUrl, string fileName);
    }

    public class ResourceDownloader : IResourceDownloader
    {
        private readonly ISettingsRepository settingsRepository;
        private readonly IHttpClient httpClient;
        private readonly IFileSystem fileSystem;

        public ResourceDownloader(ISettingsRepository settingsRepository,
            IHttpClient httpClient,
            IFileSystem fileSystem)
        {
            this.settingsRepository = settingsRepository;
            this.httpClient = httpClient;
            this.fileSystem = fileSystem;
        }

        public async Task<string> DownloadAsync(string fileUrl, string fileName)
        {
            var httpRequest = new HttpRequest
            {
                Url = fileUrl,
                Method = HttpMethod.GET
            };

            var response = await httpClient.ExecuteAsStreamAsync(httpRequest);

            if (response.StatusCode != 200)
            {
                throw new Exception($"Failed with status code {response.StatusCode} to download {fileName}");
            }

            var settings = await settingsRepository.LoadSettingsAsync();

            var filePath = $"{settings.DownloadsDirectory.AbsolutePath}\\{fileName}";
            await fileSystem.WriteStreamAsync(filePath, response.Stream);

            return filePath;
        }
    }
}
