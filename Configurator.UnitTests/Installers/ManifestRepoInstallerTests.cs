using System;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Configuration;
using Configurator.Installers;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Installers;

public class ManifestRepoInstallerTests : UnitTestBase<ManifestRepoInstaller>
{
    [Fact]
    public async Task When_installing()
    {
        var settings = new Settings
        {
            Manifest = new ManifestSettings
            {
                Repo = new Uri($"https://{RandomString()}.git")
            }
        };
        GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

        IApp? capturedApp = null;
        GetMock<IAppInstaller>().Setup(x => x.InstallOrUpgradeAsync(IsAny<IApp>()))
            .Callback<IApp>(app => capturedApp = app);

        await BecauseAsync(() => ClassUnderTest.InstallAsync());

        It("installs", () =>
            capturedApp.ShouldNotBeNull().AppId.ShouldBe(settings.Manifest.Repo.ToString())
        );
    }

    [Fact]
    public async Task When_installing_and_missing_repo_setting()
    {
        var settings = new Settings
        {
            Manifest = new ManifestSettings
            {
                Repo = null
            }
        };
        GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

        var argumentException = await BecauseThrowsAsync<ArgumentException>(() => ClassUnderTest.InstallAsync());

        It("reports the missing setting",
            () => argumentException.ShouldNotBeNull().Message
                .ShouldBe($"Missing setting: {nameof(Settings.Manifest)}.{nameof(Settings.Manifest.Repo)}"));
    }
}
