using System;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.PowerShell;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests
{
    public class InitializeCommandTests : UnitTestBase<InitializeCommand>
    {
        [Fact]
        public async Task When_initializing_for_the_first_time()
        {
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Repo = RandomUri()
                },
                Git = new GitSettings
                {
                    CloneDirectory = RandomUri()
                }
            };

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync());

            It("initializes the system", () => GetMock<ISystemInitializer>().Verify(x => x.InitializeAsync()));

            It("clones the manifest repo", () =>
                {
                    GetMock<IPowerShell>().Verify(x => x.ExecuteAsync($@"
Push-Location {settings.Git.CloneDirectory.AbsolutePath}
git clone {settings.Manifest.Repo.AbsoluteUri}
Pop-Location"));
                });
        }
        
        [Fact]
        public async Task When_initializing_without_setting_manifest_repo()
        {
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Repo = null
                }
            };

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

            var exception = await BecauseThrowsAsync<Exception>(() => ClassUnderTest.ExecuteAsync());

            It("declares manifest.repo required", () =>
                exception.ShouldNotBeNull()
                    .Message.ShouldBe("The 'manifest.repo' setting must be set before invoking initialize."));
        }
    }
}
