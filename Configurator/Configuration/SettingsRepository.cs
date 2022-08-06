using System.IO;
using System.Threading.Tasks;
using Configurator.Utilities;

namespace Configurator.Configuration
{
    public interface ISettingsRepository
    {
        Task<Settings> LoadSettingsAsync();
        Task UpdateAsync(Settings settings);
    }

    public class SettingsRepository : ISettingsRepository
    {
        private readonly IFileSystem fileSystem;
        private readonly ISpecialFolders specialFolders;
        private readonly IJsonSerializer jsonSerializer;

        public static readonly string SettingsJson = "settings.json";
        public static readonly Settings EmptySettings = new Settings();

        public SettingsRepository(IFileSystem fileSystem,
            ISpecialFolders specialFolders,
            IJsonSerializer jsonSerializer)
        {
            this.fileSystem = fileSystem;
            this.specialFolders = specialFolders;
            this.jsonSerializer = jsonSerializer;
        }

        public async Task<Settings> LoadSettingsAsync()
        {
            var fullyQualifiedSettingsFilePath = Path.Combine(specialFolders.GetLocalAppDataPath(), SettingsJson);

            if (!fileSystem.Exists(fullyQualifiedSettingsFilePath))
                return EmptySettings;
            
            var serializedSettings = await fileSystem.ReadAllTextAsync(fullyQualifiedSettingsFilePath);

            return jsonSerializer.Deserialize<Settings>(serializedSettings);
        }

        public async Task UpdateAsync(Settings settings)
        {
            var fullyQualifiedSettingsFilePath = Path.Combine(specialFolders.GetLocalAppDataPath(), SettingsJson);

            var serializedSettings = jsonSerializer.Serialize(settings);

            await fileSystem.WriteAllTextAsync(fullyQualifiedSettingsFilePath, serializedSettings);
        }
    }
}
