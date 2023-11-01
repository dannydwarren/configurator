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
        var expectedInstallableFilePath = Path.Join(expectedInstallableDirectory, "app.json");
        var installableJson = RandomString();

        var expectedManifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var manifestFileJson = RandomString();

        GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ManifestRepository_V2.ManifestFile>(IsAny<string>())).Returns(new ManifestRepository_V2.ManifestFile());
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Serialize(installable)).Returns(installableJson);

        ManifestRepository_V2.ManifestFile? capturedManifestFile = null;
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Serialize(IsAny<ManifestRepository_V2.ManifestFile>()))
            .Callback<ManifestRepository_V2.ManifestFile>(manifestFile => capturedManifestFile = manifestFile)
            .Returns(manifestFileJson);

        await BecauseAsync(() => ClassUnderTest.SaveInstallableAsync(installable));

        It("creates a directory for the installable", () =>
            GetMock<IFileSystem>().Verify(x => x.CreateDirectory(expectedInstallableDirectory)));

        It("writes installable file", () =>
            GetMock<IFileSystem>().Verify(x => x.WriteAllTextAsync(expectedInstallableFilePath, installableJson)));

        It("appends app id to manifest file", () =>
        {
            capturedManifestFile.ShouldNotBeNull().Apps.Last().ShouldBe(installable.AppId);
            GetMock<IFileSystem>().Verify(x => x.WriteAllTextAsync(expectedManifestFilePath, manifestFileJson));
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
                Repo = RandomUri(),
            }
        };
        
        var manifestFilePath = Path.Join(settings.Manifest.Directory, settings.Manifest.FileName);
        var originalManifestFileJson = RandomString();
        var originalManifestFile = new ManifestRepository_V2.ManifestFile
        {
            Apps = { RandomString() }
        };

        GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

        GetMock<IFileSystem>().Setup(x => x.ReadAllTextAsync(manifestFilePath)).ReturnsAsync(originalManifestFileJson);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ManifestRepository_V2.ManifestFile>(originalManifestFileJson)).Returns(originalManifestFile);

        ManifestRepository_V2.ManifestFile? capturedManifestFile = null;
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Serialize(IsAny<ManifestRepository_V2.ManifestFile>()))
            .Callback<ManifestRepository_V2.ManifestFile>(manifestFile => capturedManifestFile = manifestFile);

        await BecauseAsync(() => ClassUnderTest.SaveInstallableAsync(installable));

        It("appends all app ids to manifest file", () =>
            capturedManifestFile.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
            {
                x.Apps.Count.ShouldBe(2);
                x.Apps.Last().ShouldBe(installable.AppId);
            })
        );
    }

    [Fact]
    public async Task When_loading_an_empty_manifest()
    {
        var settings = new Settings
        {
            Manifest = new ManifestSettings
            {
                Directory = RandomString(),
                FileName = RandomString()
            }   
        };
        var settingsRepositoryMock = GetMock<ISettingsRepository>();
        settingsRepositoryMock.Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

        var manifestFileJson = RandomString();
        var fileSystemMock = GetMock<IFileSystem>();
        fileSystemMock
            .Setup(x => x.ReadAllTextAsync(Path.Join(settings.Manifest.Directory, settings.Manifest.FileName)))
            .ReturnsAsync(manifestFileJson);

        var manifestFile = new ManifestRepository_V2.ManifestFile();
        var jsonSerializerMock = GetMock<IHumanReadableJsonSerializer>(); 
        jsonSerializerMock.Setup(x => x.Deserialize<ManifestRepository_V2.ManifestFile>(manifestFileJson)).Returns(manifestFile);

        var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync());

        It("populates the manifest from the manifest file", () =>
        {
            manifest.AppIds.ShouldBeSameAs(manifestFile.Apps);
        });

        It("returns an empty manifest", () =>
        {
            manifest.ShouldNotBeNull()
                .ShouldSatisfyAllConditions(x =>
                {
                    x.AppIds.ShouldBeEmpty();
                    x.Apps.ShouldBeEmpty();
                });
        });
    }
}
