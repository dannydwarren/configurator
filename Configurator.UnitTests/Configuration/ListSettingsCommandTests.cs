using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Configuration
{
    public class ListSettingsCommandTests : UnitTestBase<ListSettingsCommand>
    {
        [Fact]
        public async Task When_listing_settings()
        {
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Repo = new Uri($"https://{RandomString()}")
                }
            };

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

            List<ListSettingsCommand.SettingRow>? capturedRows = null;
            GetMock<IConsoleLogger>().Setup(x => x.Table(IsAny<List<ListSettingsCommand.SettingRow>>()))
                .Callback<List<ListSettingsCommand.SettingRow>>(rows => capturedRows = rows);
            
            await BecauseAsync(() => ClassUnderTest.ExecuteAsync());
            
            It("writes settings to a console table", () =>
            {
                capturedRows.ShouldNotBeNull().ShouldNotBeEmpty();

                capturedRows.SingleOrDefault(x => x.Name == "manifest.repo").ShouldNotBeNull()
                    .ShouldSatisfyAllConditions(x =>
                    {
                        x.Value.ShouldBe(settings.Manifest.Repo.ToString());
                        x.Type.ShouldBe(settings.Manifest.Repo.GetType().ToString());
                    });
            });
        }
    }
}
