using Configurator.Apps;
using Configurator.Configuration;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.IntegrationTests
{
    public class ManifestRepository_V2Tests : IntegrationTestBase<ManifestRepository_V2>
    {
        [Fact]
        public async Task When_loading_ScriptApps()
        {
            await SetManifestFileName("script.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync());

            It($"loads basic {nameof(ScriptApp)}", () =>
            {
                manifest.Apps[0]
                    .ShouldBeOfType<ScriptApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("script-app-id");
                        x.InstallScript.ShouldBe("install-script");
                        x.VerificationScript.ShouldBe("verification-script");
                        x.UpgradeScript.ShouldBe("upgrade-script");
                        x.Configuration.ShouldBeNull();
                    });
            });

            It($"loads {nameof(ScriptApp)} with {nameof(ScriptApp.Configuration)}", () =>
            {
                manifest.Apps[1]
                    .ShouldBeOfType<ScriptApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("script-app-id-with-configuration");
                        x.Configuration.ShouldNotBeNull().RegistrySettings.ShouldHaveSingleItem()
                            .ShouldSatisfyAllConditions(y =>
                            {
                                y.KeyName.ShouldBe("key-name-test");
                                y.ValueName.ShouldBe("value-name-test");
                                y.ValueData.ShouldBe("value-data-test");
                            });
                    });
            });
        }

        [Fact]
        public async Task When_loading_ScoopApps()
        {
            await SetManifestFileName("scoop.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync());

            It($"loads basic {nameof(ScoopApp)}", () =>
            {
                manifest.Apps[0]
                    .ShouldBeOfType<ScoopApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("scoop-app-id");
                        x.InstallArgs.ShouldBeEmpty();
                        x.PreventUpgrade.ShouldBeFalse();
                        x.Configuration.ShouldBeNull();
                    });
            });

            It($"loads {nameof(ScoopApp)} with {nameof(ScoopApp.InstallArgs)}", () =>
            {
                manifest.Apps[1]
                    .ShouldBeOfType<ScoopApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("scoop-app-id-with-install-args");
                        x.InstallArgs.ShouldBe(" install-args");
                    });
            });

            It($"loads {nameof(ScoopApp)} with {nameof(ScoopApp.PreventUpgrade)}", () =>
            {
                manifest.Apps[2]
                    .ShouldBeOfType<ScoopApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("scoop-app-id-with-prevent-upgrade");
                        x.PreventUpgrade.ShouldBeTrue();
                    });
            });

            It($"loads {nameof(ScoopApp)} with {nameof(ScoopApp.Configuration)}", () =>
            {
                manifest.Apps[3]
                    .ShouldBeOfType<ScoopApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("scoop-app-id-with-configuration");
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

        [Fact]
        public async Task When_loading_WingetApps()
        {
            await SetManifestFileName("winget.manifest.json");

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

        private async Task SetManifestFileName(string manifestFileName)
        {
            var settingsRepository = GetInstance<ISettingsRepository>();
            var settings = await settingsRepository.LoadSettingsAsync();
            settings.Manifest.FileName = manifestFileName;
            await settingsRepository.SaveAsync(settings);
        }
    }
}
