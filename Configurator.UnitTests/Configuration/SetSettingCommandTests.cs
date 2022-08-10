using System;
using System.Threading.Tasks;
using Configurator.Configuration;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Configuration
{
    public class SetSettingCommandTests : UnitTestBase<SetSettingCommand>
    {
        [Fact]
        public async Task When_updating_manifest_repo()
        {
            var settingName = "manifest.repo";
            var settingValue = new Uri($"https://{RandomString()}");

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(new Settings());
            
            Settings? capturedSettings = null;
            GetMock<ISettingsRepository>().Setup(x => x.SaveAsync(IsAny<Settings>()))
                .Callback<Settings>(settings => capturedSettings = settings);
            
            await BecauseAsync(() => ClassUnderTest.ExecuteAsync(settingName, settingValue.ToString()));
            
            It("updates settings", () =>
            {
                capturedSettings.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.Manifest.Repo.ShouldBe(settingValue);
                });
            });
        }

        [Fact]
        public async Task When_updating_manifest_filename()
        {
            var settingName = "manifest.filename";
            var settingValue = RandomString();

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(new Settings());
            
            Settings? capturedSettings = null;
            GetMock<ISettingsRepository>().Setup(x => x.SaveAsync(IsAny<Settings>()))
                .Callback<Settings>(settings => capturedSettings = settings);
            
            await BecauseAsync(() => ClassUnderTest.ExecuteAsync(settingName, settingValue.ToString()));
            
            It("updates settings", () =>
            {
                capturedSettings.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.Manifest.FileName.ShouldBe(settingValue);
                });
            });
        }

        [Fact]
        public async Task When_updating_git_clonedirectory()
        {
            var settingName = "git.clonedirectory";
            var settingValue = new Uri($@"C:\{RandomString()}");

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(new Settings());
            
            Settings? capturedSettings = null;
            GetMock<ISettingsRepository>().Setup(x => x.SaveAsync(IsAny<Settings>()))
                .Callback<Settings>(settings => capturedSettings = settings);
            
            await BecauseAsync(() => ClassUnderTest.ExecuteAsync(settingName, settingValue.ToString()));
            
            It("updates settings", () =>
            {
                capturedSettings.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.Git.CloneDirectory.ShouldBe(settingValue);
                });
            });
        }
    }
}
