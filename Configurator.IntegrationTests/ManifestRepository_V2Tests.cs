using Configurator.Apps;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.IntegrationTests
{
    public class ManifestRepository_V2Tests : IntegrationTestBase<ManifestRepository_V2>
    {
        [Fact]
        public async Task When_loading()
        {
            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync());

            It("loads basic winget app", () =>
            {
                manifest.Apps[0]
                    .ShouldBeOfType<WingetApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("winget-app-id");
                        x.InstallArgs.ShouldBeEmpty();
                        x.PreventUpgrade.ShouldBeFalse();
                        x.Configuration.ShouldBeNull();
                    });
            });
        }
    }
}
