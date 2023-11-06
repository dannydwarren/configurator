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

            It($"loads basic {nameof(WingetApp)}", () =>
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

            It($"loads {nameof(WingetApp)} with {nameof(WingetApp.InstallArgs)}", () =>
            {
                manifest.Apps[1]
                    .ShouldBeOfType<WingetApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("winget-app-id-with-install-args");
                        x.InstallArgs.ShouldBe(" --override install-args");
                    });
            });

            It($"loads {nameof(WingetApp)} with {nameof(WingetApp.PreventUpgrade)}", () =>
            {
                manifest.Apps[2]
                    .ShouldBeOfType<WingetApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("winget-app-id-with-prevent-upgrade");
                        x.PreventUpgrade.ShouldBeTrue();
                    });
            });

            It($"loads {nameof(WingetApp)} with {nameof(WingetApp.Configuration)}", () =>
            {
                manifest.Apps[3]
                    .ShouldBeOfType<WingetApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("winget-app-id-with-configuration");
                        x.Configuration.RegistrySettings.ShouldHaveSingleItem()
                            .ShouldSatisfyAllConditions(y =>
                            {
                                y.KeyName.ShouldBe("key-name-test");
                                y.ValueName.ShouldBe("value-name-test");
                                y.ValueData.ShouldBe("value-data-test");
                            });
                    });
            });

        }
    }
}
