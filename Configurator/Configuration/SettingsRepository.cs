using System.IO;
using System.Threading.Tasks;
using Configurator.Utilities;

namespace Configurator.Configuration
{
    public interface ISettingsRepository
    {
        Task<Settings> LoadSettingsAsync();
        Task SaveAsync(Settings settings);
    }

    public class SettingsRepository : ISettingsRepository
    {
        private readonly IFileSystem fileSystem;
        private readonly ISpecialFolders specialFolders;
        private readonly IJsonSerializer jsonSerializer;
        private readonly string fullyQualifiedSettingsFilePath;

        public static readonly string SettingsJson = "settings.json";
        public static readonly Settings EmptySettings = new Settings();

        public SettingsRepository(IFileSystem fileSystem,
            ISpecialFolders specialFolders,
            IJsonSerializer jsonSerializer)
        {
            this.fileSystem = fileSystem;
            this.specialFolders = specialFolders;
            this.jsonSerializer = jsonSerializer;
            
            fullyQualifiedSettingsFilePath = Path.Combine(specialFolders.GetLocalAppDataPath(), SettingsJson);
        }

        public async Task<Settings> LoadSettingsAsync()
        {
            await EnsureSettingsExistAsync();

            return await LoadExistingSettingsAsync();
        }

        private async Task<Settings> LoadExistingSettingsAsync()
        {
            var serializedSettings = await fileSystem.ReadAllTextAsync(fullyQualifiedSettingsFilePath);
            return jsonSerializer.Deserialize<Settings>(serializedSettings);
        }

        private async Task EnsureSettingsExistAsync()
        {
            if (!fileSystem.Exists(fullyQualifiedSettingsFilePath))
            {
                fileSystem.CreateDirectory(specialFolders.GetLocalAppDataPath());
                await WriteSettingsAsync(EmptySettings);
            }

            EnsureDownloadsDirectoryExists(EmptySettings);
        }

        public async Task SaveAsync(Settings settings)
        {
            await WriteSettingsAsync(settings);
            EnsureDownloadsDirectoryExists(settings);
        }

        private void EnsureDownloadsDirectoryExists(Settings settings)
        {
            fileSystem.CreateDirectory(settings.DownloadsDirectory.AbsolutePath);
        }

        private async Task WriteSettingsAsync(Settings settings)
        {
            var serializedSettings = jsonSerializer.Serialize(settings);

            await fileSystem.WriteAllTextAsync(fullyQualifiedSettingsFilePath, serializedSettings);
        }
    }
}
