using Configurator.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Configurator.IntegrationTests.Configuration
{
    public class InMemorySettingsRepository : ISettingsRepository
    {
        public async Task<Settings> LoadSettingsAsync()
        {
            string executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string testManifestsDirectory = Path.Combine(executingDirectory, "..\\..\\..\\TestManifests");

            return new Settings
            {
                Manifest =
                {
                    Directory = testManifestsDirectory,
                    FileName = "test.manifest.json",
                }
            };
        }

        public async Task SaveAsync(Settings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}
