using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Configurator.Apps;
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
        var loadAppTasks = manifestFile.Apps.Select(appId => LoadAppAsync(appId, settings));
        var apps = await Task.WhenAll(loadAppTasks);
        var knownApps = apps.Where(x => x != null).ToList();

        return new Manifest_V2
        {
            AppIds = manifestFile.Apps,
            Apps = knownApps
        };
    }

    private async Task<IApp> LoadAppAsync(string appId, Settings settings)
    {
        var installableAppFilePath = Path.Join(settings.Manifest.Directory, "apps", appId, "app.json");
        var installableAppFileJson = await fileSystem.ReadAllTextAsync(installableAppFilePath);
        var rawInstallable = jsonSerializer.Deserialize<RawInstallable>(installableAppFileJson);
        rawInstallable.AppData = JsonDocument.Parse(new MemoryStream(Encoding.UTF8.GetBytes(installableAppFileJson))).RootElement;

        return ParseApp(rawInstallable);
    }

    private IApp ParseApp(RawInstallable rawInstallable)
    {
        return rawInstallable switch
        {
            { AppType: AppType.Gitconfig } => jsonSerializer.Deserialize<GitconfigApp>(rawInstallable.AppData.ToString()),
            { AppType: AppType.NonPackageApp } => jsonSerializer.Deserialize<NonPackageApp>(rawInstallable.AppData.ToString()),
            { AppType: AppType.PowerShellAppPackage } => jsonSerializer.Deserialize<PowerShellAppPackage>(rawInstallable.AppData.ToString()),
            { AppType: AppType.PowerShellModule } => jsonSerializer.Deserialize<PowerShellModuleApp>(rawInstallable.AppData.ToString()),
            { AppType: AppType.Scoop } => jsonSerializer.Deserialize<ScoopApp>(rawInstallable.AppData.ToString()),
            { AppType: AppType.ScoopBucket } => jsonSerializer.Deserialize<ScoopBucketApp>(rawInstallable.AppData.ToString()),
            { AppType: AppType.Script } => jsonSerializer.Deserialize<ScriptApp>(rawInstallable.AppData.ToString()),
            { AppType: AppType.VisualStudioExtension } => jsonSerializer.Deserialize<VisualStudioExtensionApp>(rawInstallable.AppData.ToString()),
            { AppType: AppType.Winget } => jsonSerializer.Deserialize<WingetApp>(rawInstallable.AppData.ToString()),
            _ => null!
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

    public class RawInstallable
    {
        public string AppId { get; set; }
        public AppType AppType { get; set; }
        public string Environments { get; set; }
        public JsonElement AppData { get; set; }
    }
}

public class Installable
{
    public string AppId { get; set; }
    public AppType AppType { get; set; }
    public string Environments { get; set; }
}
