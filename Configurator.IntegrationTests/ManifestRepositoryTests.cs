using Configurator.Apps;
using Configurator.Configuration;
using Configurator.Utilities;
using Configurator.Windows;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.IntegrationTests
{
    public class ManifestRepositoryTests : IntegrationTestBase<ManifestRepository>
    {
        [Fact]
        public async Task When_saving_an_installable_to_a_new_manifest()
        {
            var specialFolders = GetInstance<ISpecialFolders>();
            var settingsRepository = GetInstance<ISettingsRepository>();
            var settings = await settingsRepository.LoadSettingsAsync();
            settings.Manifest.Directory = $"{specialFolders.GetLocalAppDataPath()}/temp/integration-tests";
            Directory.CreateDirectory(settings.Manifest.Directory);
            settings.Manifest.FileName = $"{RandomString()}.manifest.json";
            await settingsRepository.SaveAsync(settings);

            var installable = new Installable
            {
                AppId = RandomString(),
                AppType = AppType.Winget,
                Environments = RandomString(),
            };

            await BecauseAsync(() => ClassUnderTest.SaveInstallableAsync(installable));

            var manifest = await ClassUnderTest.LoadAsync(new List<string>());

            It("saves", () =>
            {
                manifest.AppIds.ShouldHaveSingleItem().ShouldBe(installable.AppId);
                manifest.Apps.ShouldHaveSingleItem().ShouldSatisfyAllConditions(x =>
                {
                    x.AppId.ShouldBe(installable.AppId);
                    x.ShouldBeOfType<WingetApp>();
                });
            });

            Directory.Delete(settings.Manifest.Directory, recursive: true);
        }

        [Fact]
        public async Task When_saving_an_installable_to_an_existing_manifest()
        {
            var specialFolders = GetInstance<ISpecialFolders>();
            var settingsRepository = GetInstance<ISettingsRepository>();
            var settings = await settingsRepository.LoadSettingsAsync();
            settings.Manifest.Directory = $"{specialFolders.GetLocalAppDataPath()}/temp/integration-tests";
            Directory.CreateDirectory(settings.Manifest.Directory);
            settings.Manifest.FileName = $"{RandomString()}.manifest.json";
            await settingsRepository.SaveAsync(settings);

            var installable1 = new Installable
            {
                AppId = RandomString(),
                AppType = AppType.Winget,
                Environments = RandomString(),
            };

            await ClassUnderTest.SaveInstallableAsync(installable1);

            var installable2 = new Installable
            {
                AppId = RandomString(),
                AppType = AppType.Script,
                Environments = RandomString(),
            };

            await BecauseAsync(() => ClassUnderTest.SaveInstallableAsync(installable2));

            var manifest = await ClassUnderTest.LoadAsync(new List<string>());

            It("saves the new app and keeps the first", () =>
            {
                manifest.AppIds[0].ShouldBe(installable1.AppId);
                manifest.Apps[0].ShouldSatisfyAllConditions(x =>
                {
                    x.AppId.ShouldBe(installable1.AppId);
                    x.ShouldBeOfType<WingetApp>();
                });

                manifest.AppIds[1].ShouldBe(installable2.AppId);
                manifest.Apps[1].ShouldSatisfyAllConditions(x =>
                {
                    x.AppId.ShouldBe(installable2.AppId);
                    x.ShouldBeOfType<ScriptApp>();
                });
            });

            Directory.Delete(settings.Manifest.Directory, recursive: true);
        }

        [Fact]
        public async Task When_saving_an_installable_twice_no_changes_should_be_made()
        {
            var specialFolders = GetInstance<ISpecialFolders>();
            var settingsRepository = GetInstance<ISettingsRepository>();
            var settings = await settingsRepository.LoadSettingsAsync();
            settings.Manifest.Directory = $"{specialFolders.GetLocalAppDataPath()}/temp/integration-tests";
            Directory.CreateDirectory(settings.Manifest.Directory);
            settings.Manifest.FileName = $"{RandomString()}.manifest.json";
            await settingsRepository.SaveAsync(settings);

            var originalInstallable = new Installable
            {
                AppId = RandomString(),
                AppType = AppType.Winget,
                Environments = RandomString(),
            };

            await ClassUnderTest.SaveInstallableAsync(originalInstallable);

            var modifiedInstallable = new Installable
            {
                AppId = originalInstallable.AppId,
                AppType = AppType.Script,
                Environments = RandomString(),
            };

            await BecauseAsync(() => ClassUnderTest.SaveInstallableAsync(modifiedInstallable));

            var manifest = await ClassUnderTest.LoadAsync(new List<string>());

            It("does not save", () =>
            {
                manifest.AppIds.ShouldHaveSingleItem().ShouldBe(originalInstallable.AppId);
                manifest.Apps.ShouldHaveSingleItem().ShouldSatisfyAllConditions(x =>
                {
                    x.AppId.ShouldBe(originalInstallable.AppId);
                    x.ShouldBeOfType<WingetApp>();                    
                });
            });

            Directory.Delete(settings.Manifest.Directory, recursive: true);
        }

        [Fact]
        public async Task When_loading_single_app()
        {
            await SetManifestFileName("multiple-apps.manifest.json");
           
            var appId = "environment-1-and-3-app-id-3";

            var app = await BecauseAsync(() => ClassUnderTest.LoadAppAsync(appId));

            It("loads app matching the specifiec appId", () =>
            {
                app.AppId.ShouldBe(appId);
            });
        }

        [Fact]
        public async Task When_loading_Gitconfigs()
        {
            await SetManifestFileName("gitconfig.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It($"loads basic {nameof(GitconfigApp)}", () =>
            {
                manifest.Apps.ShouldHaveSingleItem()
                    .ShouldBeOfType<GitconfigApp>().AppId.ShouldBe("gitconfig-app-id");
            });
        }

        [Fact]
        public async Task When_loading_NonPackageApps()
        {
            await SetManifestFileName("non-package.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It($"loads basic {nameof(NonPackageApp)}", () =>
            {
                manifest.Apps.ShouldHaveSingleItem()
                    .ShouldBeOfType<NonPackageApp>().AppId.ShouldBe("non-package-app-id");
            });
        }

        [Fact]
        public async Task When_loading_PowerShellAppPackages()
        {
            await SetManifestFileName("power-shell-app-packages.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It($"loads basic {nameof(PowerShellAppPackage)}", () =>
            {
                manifest.Apps.First()
                    .ShouldBeOfType<PowerShellAppPackage>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("power-shell-app-package-app-id");
                        x.Downloader.ShouldBe("some-downloader");
                        x.DownloaderArgs.ToString().ShouldNotBeEmpty();
                        x.PreventUpgrade.ShouldBeFalse();
                    });
            });

            It($"loads {nameof(PowerShellAppPackage)} with {nameof(PowerShellAppPackage.PreventUpgrade)}", () =>
            {
                manifest.Apps.Last()
                    .ShouldBeOfType<PowerShellAppPackage>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("power-shell-app-package-app-id-with-prevent-upgrade");
                        x.Downloader.ShouldBe("some-downloader");
                        x.DownloaderArgs.ToString().ShouldNotBeEmpty();
                        x.PreventUpgrade.ShouldBe(true);
                    });
            });
        }

        [Fact]
        public async Task When_loading_PowerShellModuleApps()
        {
            await SetManifestFileName("power-shell-module.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It($"loads basic {nameof(PowerShellModuleApp)}", () =>
            {
                manifest.Apps[0]
                    .ShouldBeOfType<PowerShellModuleApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("power-shell-module-app-id");
                        x.InstallArgs.ShouldBeEmpty();
                        x.PreventUpgrade.ShouldBeFalse();
                        x.Configuration.ShouldBeNull();
                    });
            });

            It($"loads {nameof(PowerShellModuleApp)} with {nameof(PowerShellModuleApp.InstallArgs)}", () =>
            {
                manifest.Apps[1]
                    .ShouldBeOfType<PowerShellModuleApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("power-shell-module-app-id-with-install-args");
                        x.InstallArgs.ShouldBe(" install-args");
                    });
            });

            It($"loads {nameof(PowerShellModuleApp)} with {nameof(PowerShellModuleApp.PreventUpgrade)}", () =>
            {
                manifest.Apps[2]
                    .ShouldBeOfType<PowerShellModuleApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("power-shell-module-app-id-with-prevent-upgrade");
                        x.PreventUpgrade.ShouldBeTrue();
                    });
            });

            It($"loads {nameof(PowerShellModuleApp)} with {nameof(PowerShellModuleApp.Configuration)}", () =>
            {
                manifest.Apps[3]
                    .ShouldBeOfType<PowerShellModuleApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("power-shell-module-app-id-with-configuration");
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
        public async Task When_loading_ScoopApps()
        {
            await SetManifestFileName("scoop.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

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
        public async Task When_loading_ScoopBucketApps()
        {
            await SetManifestFileName("scoop-bucket.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It($"loads basic {nameof(ScoopBucketApp)}", () =>
            {
                manifest.Apps.ShouldHaveSingleItem()
                    .ShouldBeOfType<ScoopBucketApp>().AppId.ShouldBe("scoop-bucket-app-id");
            });
        }

        [Fact]
        public async Task When_loading_ScriptApps()
        {
            await SetManifestFileName("script.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

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
        public async Task When_loading_VisualStudioExtensionApps()
        {
            await SetManifestFileName("visual-studio-extension.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It($"loads basic {nameof(VisualStudioExtensionApp)}", () =>
            {
                manifest.Apps[0]
                    .ShouldBeOfType<VisualStudioExtensionApp>().ShouldSatisfyAllConditions(x =>
                    {
                        x.AppId.ShouldBe("visual-studio-extension-app-id");
                        x.DownloaderArgs.GetProperty("Publisher").GetString().ShouldBe("publisher-1");
                        x.DownloaderArgs.GetProperty("ExtensionName").GetString().ShouldBe("extension-name-1");
                    });
            });
        }

        [Fact]
        public async Task When_loading_WingetApps()
        {
            await SetManifestFileName("winget.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

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

        [Fact]
        public async Task When_loading_UnknownApps()
        {
            await SetManifestFileName("unknown.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It("does not load unknown apps", () =>
            {
                manifest.Apps.ShouldBeEmpty();
            });
        }

        [Fact]
        public async Task When_loading_an_app_with_registry_settings()
        {
            await SetManifestFileName("registry-settings.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            var registrySettings = manifest.Apps.Single().Configuration!.RegistrySettings;

            It($"loads string {nameof(RegistrySetting.ValueData)}", () =>
            {
                registrySettings[0].ShouldSatisfyAllConditions(x =>
                {
                    x.KeyName.ShouldBe("key-1");
                    x.ValueName.ShouldBe("string");
                    x.ValueData.ShouldBe("string-data");
                });
            });

            It($"loads string {nameof(RegistrySetting.ValueData)} with environment tokens", () =>
            {
                registrySettings[1].ShouldSatisfyAllConditions(x =>
                {
                    x.KeyName.ShouldBe("key-2");
                    x.ValueName.ShouldBe("string");
                    x.ValueData.ShouldBe("C:\\Program Files\\string-data");
                });
            });

            It($"loads int {nameof(RegistrySetting.ValueData)}", () =>
            {
                registrySettings[2].ShouldSatisfyAllConditions(x =>
                {
                    x.KeyName.ShouldBe("key-3");
                    x.ValueName.ShouldBe("uint");
                    x.ValueData.ShouldBeOfType<uint>().ShouldBe((uint)42);
                });
            });
        }

        [Fact]
        public async Task When_loading_apps_with_no_specified_enviroments()
        {
            await SetManifestFileName("multiple-apps.manifest.json");

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(new List<string>()));

            It("loads all apps", () =>
            {
                manifest.Apps.Count.ShouldBe(5);
            });
        }

        [Fact]
        public async Task When_loading_apps_for_a_specific_environment()
        {
            await SetManifestFileName("multiple-apps.manifest.json");

            var environments = new List<string>
            {
                "environment2"
            };

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(environments));

            It("only loads apps matching the specified environment, ignoring case", () =>
            {
                manifest.Apps.Count.ShouldBe(1);
            });
        }

        [Fact]
        public async Task When_loading_apps_for_multiple_environments()
        {
            await SetManifestFileName("multiple-apps.manifest.json");

            var environments = new List<string>
            {
                "Environment2",
                "Environment3"
            };

            var manifest = await BecauseAsync(() => ClassUnderTest.LoadAsync(environments));

            It("only loads apps matching the specified environments", () =>
            {
                manifest.Apps.Count.ShouldBe(3);
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
