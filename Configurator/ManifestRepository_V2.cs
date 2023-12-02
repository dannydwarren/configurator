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
    Task<Manifest_V2> LoadAsync(List<string> specifiedEnvironments);
    Task<IApp> LoadAppAsync(string appId);
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

    public async Task<Manifest_V2> LoadAsync(List<string> specifiedEnvironments)
    {
        return await LoadManifestAsync(specifiedEnvironments);
    }

    public async Task<IApp> LoadAppAsync(string appId)
    {
        var manifest = await LoadManifestAsync(new List<string>());
        var app = manifest.Apps.First(x => x.AppId == appId);
        
        return app;
    }

    private async Task<Manifest_V2> LoadManifestAsync(List<string> specifiedEnvironments)
    {
        var settings = await settingsRepository.LoadSettingsAsync();
        var manifestFile = await LoadManifestFileAsync(settings);
        var loadInstallableTasks = manifestFile.Apps.Select(appId => LoadInstallableAsync(appId, settings));
        var installables = await Task.WhenAll(loadInstallableTasks);
        var apps = installables
            .Where(installable => IncludeForSpecifiedEnvironments(specifiedEnvironments, installable))
            .Select(ParseApp)
            .Where(x => x != null)
            .ToList();

        return new Manifest_V2
        {
            AppIds = manifestFile.Apps,
            Apps = apps
        };
    }


    private static bool IncludeForSpecifiedEnvironments(List<string> specifiedEnvironments, RawInstallable installable)
    {
        var installableEnvironmentsLowered = installable.Environments.ToLower();

        var noEnvironmentsHaveBeenSpecified = !specifiedEnvironments.Any();
        var installableTargetsASpecifiedEnvironment = specifiedEnvironments.Any(env => installableEnvironmentsLowered.Contains(env.ToLower()));

        return noEnvironmentsHaveBeenSpecified || installableTargetsASpecifiedEnvironment;
    }

    private async Task<RawInstallable> LoadInstallableAsync(string appId, Settings settings)
    {
        var installableAppFilePath = Path.Join(settings.Manifest.Directory, "apps", appId, "app.json");
        var installableAppFileJson = await fileSystem.ReadAllTextAsync(installableAppFilePath);
        var rawInstallable = jsonSerializer.Deserialize<RawInstallable>(installableAppFileJson);
        rawInstallable.AppData = JsonDocument.Parse(new MemoryStream(Encoding.UTF8.GetBytes(installableAppFileJson))).RootElement;

        return rawInstallable;
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

        if (CheckInstallableAlreadyInManifest(installable, manifestFile))
            return;

        var installableDirectory = CreateInstallableDirectoryAsync(installable, settings);
        await WriteInstallableFileAsync(installable, installableDirectory);

        manifestFile.Apps.Add(installable.AppId);

        await WriteManifestFileAsync(settings, manifestFile);
    }

    private bool CheckInstallableAlreadyInManifest(Installable installable, ManifestFile manifestFile)
    {
        return manifestFile.Apps.Contains(installable.AppId);
    }

    private async Task<ManifestFile> LoadManifestFileAsync(Settings settings)
    {
        var manifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        await EnsureManifestFileExists(manifestFilePath);
        var manifestFileJson = await fileSystem.ReadAllTextAsync(manifestFilePath);
        return jsonSerializer.Deserialize<ManifestFile>(manifestFileJson);
    }

    private async Task EnsureManifestFileExists(string fileName)
    {
        if (fileSystem.Exists(fileName))
            return;

        fileSystem.CreateFile(fileName);
        await fileSystem.WriteAllTextAsync(fileName, "{}");
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
