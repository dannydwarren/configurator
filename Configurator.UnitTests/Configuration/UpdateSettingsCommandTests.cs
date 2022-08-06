using System.Threading.Tasks;
using Configurator.Configuration;
using Moq;
using Xunit;

namespace Configurator.UnitTests.Configuration
{
    public class UpdateSettingsCommandTests : UnitTestBase<UpdateSettingsCommand>
    {
        [Fact]
        public async Task When_updating_settings()
        {
            var settings = new Settings();
            
            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);
            
            await BecauseAsync(() => ClassUnderTest.ExecuteAsync());
            
            It("updates settings", () => GetMock<ISettingsRepository>().Verify(x => x.UpdateAsync(settings)));
        }
    }
}
