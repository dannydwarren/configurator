using Configurator.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Configurator.IntegrationTests.Configuration
{
    public class InMemorySettingsRepository : ISettingsRepository
    {
        private Settings Settings { get; set; }

        public async Task<Settings> LoadSettingsAsync()
        {
            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string testManifestsDirectory = Path.Combine(executingDirectory, "..\\..\\..\\TestManifests");

            return Settings ??= new Settings
            {
                Manifest =
                {
                    Directory = testManifestsDirectory,
                    FileName = "test.manifest.json",
                }
            };
        }

        public Task SaveAsync(Settings settings)
        {
            Settings = settings;

            return Task.CompletedTask;
        }
    }
}
