using System.IO;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;

namespace Configurator;

public interface IManifestRepository_V2
{
    Task SaveInstallableAsync(Installable installable);
}

public class ManifestRepository_V2 : IManifestRepository_V2
{
    private readonly ISettingsRepository settingsRepository;
    private readonly IJsonSerializer jsonSerializer;
    private readonly IFileSystem fileSystem;

    public ManifestRepository_V2(ISettingsRepository settingsRepository,
        IJsonSerializer jsonSerializer,
        IFileSystem fileSystem)
    {
        this.settingsRepository = settingsRepository;
        this.jsonSerializer = jsonSerializer;
        this.fileSystem = fileSystem;
    }
    
    public async Task SaveInstallableAsync(Installable installable)
    {
        var settings = await settingsRepository.LoadSettingsAsync();
        var manifest = await LoadManifestAsync(settings);
        
        var installableDirectory = CreateInstallableDirectoryAsync(installable, settings);
        await WriteInstallableFileAsync(installable, installableDirectory);

        manifest.Apps.Add(installable.AppId);
        
        await WriteManifestFileAsync(settings, manifest);
    }

    private async Task<Manifest_V2> LoadManifestAsync(Settings settings)
    {
        var manifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var manifestJson = await fileSystem.ReadAllTextAsync(manifestFilePath);
        return jsonSerializer.Deserialize<Manifest_V2>(manifestJson);
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

    private async Task WriteManifestFileAsync(Settings settings, Manifest_V2 manifest)
    {
        var manifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var manifestJson = jsonSerializer.Serialize(manifest);
        await fileSystem.WriteAllTextAsync(manifestFilePath, manifestJson);
    }
}

public class Installable
{
    public string AppId { get; set; }
    public AppType AppType { get; set; }
    public string Environments { get; set; }
}
