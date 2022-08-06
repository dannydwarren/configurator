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

        public SettingsRepositoryTests()
        {
            var localAppDataPath = RandomString();
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
            GetMock<IFileSystem>()
                .Setup(x => x.Exists(fullyQualifiedSettingsFilePath))
                .Returns(false);

            var settings = await BecauseAsync(() => ClassUnderTest.LoadSettingsAsync());
            
            It("returns empty settings", () => settings.ShouldBeSameAs(SettingsRepository.EmptySettings));
        }
        
        [Fact]
        public async Task When_updating_settings()
        {
            var settings = new Settings();
            var settingsJson = RandomString();

            GetMock<IJsonSerializer>().Setup(x => x.Serialize(settings)).Returns(settingsJson);

            await BecauseAsync(() => ClassUnderTest.UpdateAsync(settings));

            It("writes to local application data", () =>
            {
                GetMock<IFileSystem>()
                    .Verify(x => x.WriteAllTextAsync(fullyQualifiedSettingsFilePath, settingsJson));
            });
        }
    }
}
