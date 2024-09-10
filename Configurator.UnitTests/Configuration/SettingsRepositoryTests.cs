using System.IO;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Configuration
{
    public class SettingsRepositoryTests : UnitTestBase<SettingsRepository>
    {
        private readonly string fullyQualifiedSettingsFilePath;
        private readonly string localAppDataPath;

        public SettingsRepositoryTests()
        {
            localAppDataPath = RandomString();
            fullyQualifiedSettingsFilePath = Path.Combine(localAppDataPath, SettingsRepository.SettingsJson);
            
            GetMock<ISpecialFolders>().Setup(x => x.GetLocalAppDataPath()).Returns(localAppDataPath);
        }
        
        [Fact]
        public async Task When_loading_existing_settings()
        {
            var settingsJson = RandomString();
            var expectedSettings = new Settings();

            GetMock<IFileSystem>()
                .Setup(x => x.Exists(fullyQualifiedSettingsFilePath))
                .Returns(true);

            GetMock<IFileSystem>()
                .Setup(x => x.ReadAllTextAsync(fullyQualifiedSettingsFilePath))
                .ReturnsAsync(settingsJson);

            GetMock<IJsonSerializer>().Setup(x => x.Deserialize<Settings>(settingsJson)).Returns(expectedSettings);

            var settings = await BecauseAsync(() => ClassUnderTest.LoadSettingsAsync());
            
            It("returns existing settings", () => settings.ShouldBeSameAs(expectedSettings));
        }
        
        [Fact]
        public async Task When_loading_settings_for_the_first_time()
        {
            var settingsJson = RandomString();
            var expectedSettings = new Settings();
            
            GetMock<IFileSystem>()
                .Setup(x => x.Exists(fullyQualifiedSettingsFilePath))
                .Returns(false);
            
            GetMock<IJsonSerializer>().Setup(x => x.Serialize(SettingsRepository.EmptySettings)).Returns(settingsJson);

            GetMock<IFileSystem>()
                .Setup(x => x.ReadAllTextAsync(fullyQualifiedSettingsFilePath))
                .ReturnsAsync(settingsJson);

            GetMock<IJsonSerializer>().Setup(x => x.Deserialize<Settings>(settingsJson)).Returns(expectedSettings);
            
            var settings = await BecauseAsync(() => ClassUnderTest.LoadSettingsAsync());
            
            It("ensures settings exist", () =>
            {
                GetMock<IFileSystem>()
                    .Verify(x => x.CreateDirectory(localAppDataPath));
                
                GetMock<IFileSystem>()
                    .Verify(x => x.WriteAllTextAsync(fullyQualifiedSettingsFilePath, settingsJson));
            });

            It($"ensures the {nameof(Settings.DownloadsDirectory)} exists", () =>
            {
                GetMock<IFileSystem>()
                    .Verify(x => x.CreateDirectory(settings.DownloadsDirectory.AbsolutePath));
            });

            It("returns newly written settings", () => settings.ShouldBeSameAs(expectedSettings));
        }
        
        [Fact]
        public async Task When_saving_settings()
        {
            var settings = new Settings();
            var settingsJson = RandomString();

            GetMock<IJsonSerializer>().Setup(x => x.Serialize(settings)).Returns(settingsJson);

            await BecauseAsync(() => ClassUnderTest.SaveAsync(settings));

            It("writes to local application data", () =>
            {
                GetMock<IFileSystem>()
                    .Verify(x => x.WriteAllTextAsync(fullyQualifiedSettingsFilePath, settingsJson));
            });

            It($"ensures the {nameof(Settings.DownloadsDirectory)} exists", () =>
            {
                GetMock<IFileSystem>()
                    .Verify(x => x.CreateDirectory(settings.DownloadsDirectory.AbsolutePath));
            });
        }
    }
}
