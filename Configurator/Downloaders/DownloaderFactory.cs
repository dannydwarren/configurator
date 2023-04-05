using System;
using System.Diagnostics.CodeAnalysis;

namespace Configurator.Downloaders
{
    public interface IDownloaderFactory
    {
        IDownloader GetDownloader(string downloaderName);
    }

    public class DownloaderFactory : IDownloaderFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DownloaderFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Only references project types")]
        public IDownloader GetDownloader(string downloaderName)
        {
            var type = Type.GetType($"{typeof(IDownloader).Namespace}.{downloaderName}");

            if (type == null)
            {
                throw new Exception(
                    $"Cannot find downloader '{downloaderName}' in the namespace '{typeof(IDownloader).Namespace}'");
            }

            return (IDownloader)serviceProvider.GetService(type)!;
        }
    }
}
