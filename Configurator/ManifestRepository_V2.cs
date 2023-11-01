using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;

namespace Configurator;

public interface IManifestRepository_V2
{
    Task<Manifest_V2> LoadAsync();
    Task SaveInstallableAsync(Installable installable);
}

public class ManifestRepository_V2 : IManifestRepository_V2
{
    private readonly ISettingsRepository settingsRepository;
    private readonly IHumanReadableJsonSerializer jsonSerializer;
    private readonly IFileSystem fileSystem;

    public ManifestRepository_V2(ISettingsRepository settingsRepository,
        IHumanReadableJsonSerializer jsonSerializer,
        IFileSystem fileSystem)
    {
        this.settingsRepository = settingsRepository;
        this.jsonSerializer = jsonSerializer;
        this.fileSystem = fileSystem;
    }

    public async Task<Manifest_V2> LoadAsync()
    {
        var settings = await settingsRepository.LoadSettingsAsync();
        var manifestFile = await LoadManifestFileAsync(settings);

        return new Manifest_V2
        {
            AppIds = manifestFile.Apps
        };
    }

    public async Task SaveInstallableAsync(Installable installable)
    {
        var settings = await settingsRepository.LoadSettingsAsync();
        var manifestFile = await LoadManifestFileAsync(settings);
        
        var installableDirectory = CreateInstallableDirectoryAsync(installable, settings);
        await WriteInstallableFileAsync(installable, installableDirectory);

        manifestFile.Apps.Add(installable.AppId);
        
        await WriteManifestFileAsync(settings, manifestFile);
    }

    private async Task<ManifestFile> LoadManifestFileAsync(Settings settings)
    {
        var manifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var manifestFileJson = await fileSystem.ReadAllTextAsync(manifestFilePath);
        return jsonSerializer.Deserialize<ManifestFile>(manifestFileJson);
    }
    
    private string CreateInstallableDirectoryAsync(Installable installable, Settings settings)
    {
        var installableDirectory = Path.Join(settings.Manifest.Directory, "apps", installable.AppId);

        fileSystem.CreateDirectory(installableDirectory);
        return installableDirectory;
    }

    private async Task WriteInstallableFileAsync(Installable installable, string installableDirectory)
    {
        var installableJson = jsonSerializer.Serialize(installable);
        var installableFilePath = Path.Join(installableDirectory, "app.json");

        await fileSystem.WriteAllTextAsync(installableFilePath, installableJson);
    }

    private async Task WriteManifestFileAsync(Settings settings, ManifestFile manifestFile)
    {
        var manifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var manifestFileJson = jsonSerializer.Serialize(manifestFile);
        await fileSystem.WriteAllTextAsync(manifestFilePath, manifestFileJson);
    }

    public class ManifestFile
    {
        public List<string> Apps { get; set; } = new List<string>();
    }
}

public class Installable
{
    public string AppId { get; set; }
    public AppType AppType { get; set; }
    public string Environments { get; set; }
}
