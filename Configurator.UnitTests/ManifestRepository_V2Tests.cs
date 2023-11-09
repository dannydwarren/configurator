using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Configuration;
using Configurator.Utilities;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests;

public class ManifestRepository_V2Tests : UnitTestBase<ManifestRepository_V2>
{
    private readonly List<ManifestRepository_V2.RawInstallable> installables;
    private readonly List<ScriptApp> knownScriptApps;

    public ManifestRepository_V2Tests()
    {
        installables = new List<ManifestRepository_V2.RawInstallable>
            {
                new ManifestRepository_V2.RawInstallable
                {
                    AppId = RandomString(),
                    AppType = AppType.Script,
                    Environments = "Personal".ToLower(),
                    AppData = JsonDocument.Parse(new MemoryStream(Encoding.UTF8.GetBytes(@"{""app"": 1}"))).RootElement
                },
                new ManifestRepository_V2.RawInstallable
                {
                    AppId = RandomString(),
                    AppType = AppType.Script,
                    Environments = "Media",
                    AppData = JsonDocument.Parse(new MemoryStream(Encoding.UTF8.GetBytes(@"{""app"": 2}"))).RootElement
                },
                new ManifestRepository_V2.RawInstallable
                {
                    AppId = RandomString(),
                    AppType = AppType.Script,
                    Environments = "Work",
                    AppData = JsonDocument.Parse(new MemoryStream(Encoding.UTF8.GetBytes(@"{""app"": 3}"))).RootElement
                },
                new ManifestRepository_V2.RawInstallable
                {
                    AppId = RandomString(),
                    AppType = AppType.Script,
                    Environments = "All",
                    AppData = JsonDocument.Parse(new MemoryStream(Encoding.UTF8.GetBytes(@"{""app"": 4}"))).RootElement
                },
                new ManifestRepository_V2.RawInstallable
                {
                    AppId = RandomString(),
                    AppType = AppType.Unknown,
                    Environments = "All",
                    AppData = JsonDocument.Parse(new MemoryStream(Encoding.UTF8.GetBytes(@"{""app"": 5}"))).RootElement
                }
            };

        knownScriptApps = new List<ScriptApp>
            {
                new ScriptApp { AppId = installables[0].AppId },
                new ScriptApp { AppId = installables[1].AppId },
                new ScriptApp { AppId = installables[2].AppId },
                new ScriptApp { AppId = installables[3].AppId },
            };
    }

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

    [Fact]
    public async Task When_loading_manifest_with_apps()
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

        var manifestFile = new ManifestRepository_V2.ManifestFile
        {
            Apps = installables.Select(x => x.AppId).ToList()
        };
        var jsonSerializerMock = GetMock<IHumanReadableJsonSerializer>();
        jsonSerializerMock.Setup(x => x.Deserialize<ManifestRepository_V2.ManifestFile>(manifestFileJson)).Returns(manifestFile);

        var installableAppFilePath1 = Path.Join(settings.Manifest.Directory, "apps", manifestFile.Apps[0], "app.json");
        var installableAppFilePath2 = Path.Join(settings.Manifest.Directory, "apps", manifestFile.Apps[1], "app.json");
        var installableAppFilePath3 = Path.Join(settings.Manifest.Directory, "apps", manifestFile.Apps[2], "app.json");
        var installableAppFilePath4 = Path.Join(settings.Manifest.Directory, "apps", manifestFile.Apps[3], "app.json");
        var installableAppFilePath5 = Path.Join(settings.Manifest.Directory, "apps", manifestFile.Apps[4], "app.json");

        var installableAppFileJson1 = $"{{\"prop1\": \"{RandomString()}\"}}";
        var installableAppFileJson2 = $"{{\"prop1\": \"{RandomString()}\"}}";
        var installableAppFileJson3 = $"{{\"prop1\": \"{RandomString()}\"}}";
        var installableAppFileJson4 = $"{{\"prop1\": \"{RandomString()}\"}}";
        var installableAppFileJson5 = $"{{\"prop1\": \"{RandomString()}\"}}";

        fileSystemMock.Setup(x => x.ReadAllTextAsync(installableAppFilePath1)).ReturnsAsync(installableAppFileJson1);
        fileSystemMock.Setup(x => x.ReadAllTextAsync(installableAppFilePath2)).ReturnsAsync(installableAppFileJson2);
        fileSystemMock.Setup(x => x.ReadAllTextAsync(installableAppFilePath3)).ReturnsAsync(installableAppFileJson3);
        fileSystemMock.Setup(x => x.ReadAllTextAsync(installableAppFilePath4)).ReturnsAsync(installableAppFileJson4);
        fileSystemMock.Setup(x => x.ReadAllTextAsync(installableAppFilePath5)).ReturnsAsync(installableAppFileJson5);

        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ManifestRepository_V2.RawInstallable>(installableAppFileJson1)).Returns(installables[0]);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ManifestRepository_V2.RawInstallable>(installableAppFileJson2)).Returns(installables[1]);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ManifestRepository_V2.RawInstallable>(installableAppFileJson3)).Returns(installables[2]);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ManifestRepository_V2.RawInstallable>(installableAppFileJson4)).Returns(installables[3]);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ManifestRepository_V2.RawInstallable>(installableAppFileJson5)).Returns(installables[4]);

        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ScriptApp>(installables[0].AppData.ToString())).Returns(knownScriptApps[0]);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ScriptApp>(installables[1].AppData.ToString())).Returns(knownScriptApps[1]);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ScriptApp>(installables[2].AppData.ToString())).Returns(knownScriptApps[2]);
        GetMock<IHumanReadableJsonSerializer>().Setup(x => x.Deserialize<ScriptApp>(installables[3].AppData.ToString())).Returns(knownScriptApps[3]);

        var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync());

        It("loads all known apps", () =>
        {
            manifest.Apps.Count.ShouldBe(knownScriptApps.Count);
        });

        It("excludes unknown apps", () =>
        {
            GetMock<IJsonSerializer>().VerifyNever(x => x.Deserialize<ScriptApp>(installables[4].AppData.ToString()!));
        });
    }
}
