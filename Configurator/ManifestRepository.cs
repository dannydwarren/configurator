using System;
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

public interface IManifestRepository
{
    Task<Manifest> LoadAsync(List<string> specifiedEnvironments);
    Task<IApp> LoadAppAsync(string appId);
    Task SaveInstallableAsync(Installable installable);
}

public class ManifestRepository : IManifestRepository
{
    private readonly ISettingsRepository settingsRepository;
    private readonly IHumanReadableJsonSerializer jsonSerializer;
    private readonly IFileSystem fileSystem;

    public ManifestRepository(ISettingsRepository settingsRepository,
        IHumanReadableJsonSerializer jsonSerializer,
        IFileSystem fileSystem)
    {
        this.settingsRepository = settingsRepository;
        this.jsonSerializer = jsonSerializer;
        this.fileSystem = fileSystem;
    }

    public async Task<Manifest> LoadAsync(List<string> specifiedEnvironments)
    {
        return await LoadManifestAsync(specifiedEnvironments);
    }

    public async Task<IApp> LoadAppAsync(string appId)
    {
        var manifest = await LoadManifestAsync(new List<string>());
        var app = manifest.Apps.First(x => x.AppId == appId);
        
        return app;
    }

    private async Task<Manifest> LoadManifestAsync(List<string> specifiedEnvironments)
    {
        var settings = await settingsRepository.LoadSettingsAsync();
        var manifestFile = await LoadManifestFileAsync(settings.Manifest);
        var loadInstallableTasks = manifestFile.Apps.Select(appId => LoadInstallableAsync(appId, settings.Manifest));
        var installables = await Task.WhenAll(loadInstallableTasks);
        var apps = installables
            .Where(installable => IncludeForSpecifiedEnvironments(specifiedEnvironments, installable))
            .Select(ParseApp)
            .Where(x => x != null)
            .Select(x => MapShellAppScripts(x, settings.Manifest))
            .Where(x => x != null)
            .ToList();

        return new Manifest
        {
            AppIds = manifestFile.Apps,
            Apps = apps
        };
    }

    private IApp MapShellAppScripts(IApp app, ManifestSettings manifestSettings)
    {
        if (app.Shell == Shell.None)
            return app;

        var appDirectory = GetAppDirectory(app.AppId, manifestSettings);
        var shellScriptExtension = GetShellScriptExtension(app);

        var installScriptFilePath = Path.Join(appDirectory, $"install{shellScriptExtension}");
        var upgradeScriptFilePath = Path.Join(appDirectory, $"upgrade{shellScriptExtension}");
        var verificationScriptFilePath = Path.Join(appDirectory, $"verification{shellScriptExtension}");

        if(!File.Exists(installScriptFilePath))
        {
            return null!;
        }

        if (app is PowerShellApp powerShellApp)
        {
            powerShellApp.InstallScript = $". \"{installScriptFilePath}\"";
            powerShellApp.UpgradeScript = fileSystem.Exists(upgradeScriptFilePath) ? $". \"{upgradeScriptFilePath}\"" : null;
            powerShellApp.VerificationScript = fileSystem.Exists(verificationScriptFilePath) ? $". \"{verificationScriptFilePath}\"" : null;
        }

        return app;
    }

    private string GetShellScriptExtension(IApp app)
    {
        return app.Shell switch
        {
            Shell.PowerShell => ".ps1",
            _ => throw new NotImplementedException(),
        };
    }

    private static bool IncludeForSpecifiedEnvironments(List<string> specifiedEnvironments, RawInstallable installable)
    {
        var installableEnvironmentsLowered = installable.Environments.ToLower();

        var noEnvironmentsHaveBeenSpecified = !specifiedEnvironments.Any();
        var installableTargetsASpecifiedEnvironment = specifiedEnvironments.Any(env => installableEnvironmentsLowered.Contains(env.ToLower()));

        return noEnvironmentsHaveBeenSpecified || installableTargetsASpecifiedEnvironment;
    }

    private async Task<RawInstallable> LoadInstallableAsync(string appId, ManifestSettings manifestSettings)
    {
        var appDirectory = GetAppDirectory(appId, manifestSettings);
        var installableAppFilePath = Path.Join(appDirectory, "app.json");
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
            { AppType: AppType.PowerShell } => jsonSerializer.Deserialize<PowerShellApp>(rawInstallable.AppData.ToString()),
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
        var manifestFile = await LoadManifestFileAsync(settings.Manifest);

        if (CheckInstallableAlreadyInManifest(installable, manifestFile))
            return;

        var installableDirectory = CreateInstallableDirectoryAsync(installable, settings.Manifest);
        await WriteInstallableFileAsync(installable, installableDirectory);

        manifestFile.Apps.Add(installable.AppId);

        await WriteManifestFileAsync(settings.Manifest, manifestFile);
    }

    private bool CheckInstallableAlreadyInManifest(Installable installable, ManifestFile manifestFile)
    {
        return manifestFile.Apps.Contains(installable.AppId);
    }

    private async Task<ManifestFile> LoadManifestFileAsync(ManifestSettings manifestSettings)
    {
        var manifestFilePath = Path.Join(manifestSettings.Directory, manifestSettings.FileName);
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

    private string CreateInstallableDirectoryAsync(Installable installable, ManifestSettings manifestSettings)
    {
        var installableDirectory = GetAppDirectory(installable.AppId, manifestSettings);

        fileSystem.CreateDirectory(installableDirectory);
        return installableDirectory;
    }

    private async Task WriteInstallableFileAsync(Installable installable, string installableDirectory)
    {
        var installableJson = jsonSerializer.Serialize(installable);
        var installableFilePath = Path.Join(installableDirectory, "app.json");

        await fileSystem.WriteAllTextAsync(installableFilePath, installableJson);
    }

    private async Task WriteManifestFileAsync(ManifestSettings manifestSettings, ManifestFile manifestFile)
    {
        var manifestFilePath = Path.Join(manifestSettings.Directory, manifestSettings.FileName);
        var manifestFileJson = jsonSerializer.Serialize(manifestFile);
        await fileSystem.WriteAllTextAsync(manifestFilePath, manifestFileJson);
    }

    private string GetAppDirectory(string appId, ManifestSettings manifestSettings)
    {
        return Path.Join(manifestSettings.Directory, "apps", appId);
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
