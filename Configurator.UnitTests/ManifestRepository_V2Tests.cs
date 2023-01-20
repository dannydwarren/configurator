using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests;

public class ManifestRepository_V2Tests : UnitTestBase<ManifestRepository_V2>
{
    [Fact]
    public async Task When_saving_installable()
    {
        var installable = new Installable
        {
            AppId = RandomString()
        };

        var settings = new Settings
        {
            Manifest = new ManifestSettings
            {
                Directory = RandomString(),
                FileName = RandomString()
            }
        };

        var expectedInstallableDirectory = Path.Join(settings.Manifest.Directory, "apps", installable.AppId);
        var expectedInstallableFilePath = Path.Join(expectedInstallableDirectory, "installable.json");
        var installableJson = RandomString();

        var expectedManifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var manifestJson = RandomString();

        GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);
        GetMock<IJsonSerializer>().Setup(x => x.Deserialize<Manifest_V2>(IsAny<string>())).Returns(new Manifest_V2());
        GetMock<IJsonSerializer>().Setup(x => x.Serialize(installable)).Returns(installableJson);

        Manifest_V2? capturedManifest = null;
        GetMock<IJsonSerializer>().Setup(x => x.Serialize(IsAny<Manifest_V2>()))
            .Callback<Manifest_V2>(manifest => capturedManifest = manifest)
            .Returns(manifestJson);

        await BecauseAsync(() => ClassUnderTest.SaveInstallableAsync(installable));

        It("creates a directory for the installable", () =>
            GetMock<IFileSystem>().Verify(x => x.CreateDirectory(expectedInstallableDirectory)));

        It("writes installable file", () =>
            GetMock<IFileSystem>().Verify(x => x.WriteAllTextAsync(expectedInstallableFilePath, installableJson)));

        It("appends app id to manifest file", () =>
        {
            capturedManifest.ShouldNotBeNull().Apps.Last().ShouldBe(installable.AppId);
            GetMock<IFileSystem>().Verify(x => x.WriteAllTextAsync(expectedManifestFilePath, manifestJson));
        });
    }

    [Fact]
    public async Task When_saving_installable_with_existing_installables()
    {
        var installable = new Installable
        {
            AppId = RandomString()
        };

        var settings = new Settings
        {
            Manifest = new ManifestSettings
            {
                Repo = new Uri($"c:/{RandomString()}"),
            }
        };
        
        var manifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var originalManifestJson = RandomString();
        var originalManifest = new Manifest_V2
        {
            Apps = { RandomString() }
        };

        GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

        GetMock<IFileSystem>().Setup(x => x.ReadAllTextAsync(manifestFilePath)).ReturnsAsync(originalManifestJson);
        GetMock<IJsonSerializer>().Setup(x => x.Deserialize<Manifest_V2>(originalManifestJson)).Returns(originalManifest);

        Manifest_V2? capturedManifest = null;
        GetMock<IJsonSerializer>().Setup(x => x.Serialize(IsAny<Manifest_V2>()))
            .Callback<Manifest_V2>(manifest => capturedManifest = manifest);

        await BecauseAsync(() => ClassUnderTest.SaveInstallableAsync(installable));

        It("appends all app ids to manifest file", () =>
            capturedManifest.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
            {
                x.Apps.Count.ShouldBe(2);
                x.Apps.Last().ShouldBe(installable.AppId);
            })
        );
    }
}
