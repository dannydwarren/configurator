using System;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Configuration;

namespace Configurator.Installers;

public interface IManifestRepoInstaller
{
    Task InstallAsync();
}

public class ManifestRepoInstaller : IManifestRepoInstaller
{
    private readonly IAppInstaller appInstaller;
    private readonly ISettingsRepository settingsRepository;

    public ManifestRepoInstaller(IAppInstaller appInstaller, ISettingsRepository settingsRepository)
    {
        this.appInstaller = appInstaller;
        this.settingsRepository = settingsRepository;
    }

    public async Task InstallAsync()
    {
        var settings = await settingsRepository.LoadSettingsAsync();
        var repoUri = settings.Manifest.Repo;
        if (repoUri == null)
        {
            throw new ArgumentException(
                $"Missing setting: {nameof(Settings.Manifest)}.{nameof(Settings.Manifest.Repo)}");
        }

        await appInstaller.InstallOrUpgradeAsync(new GitRepoApp
        {
            AppId = repoUri.ToString(),
            CloneRootDirectory = settings.Git.CloneDirectory.AbsolutePath
        });
    }
}
