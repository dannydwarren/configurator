using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Configuration;
using Configurator.Installers;
using Configurator.PowerShell;
using Configurator.Utilities;
using Configurator.Windows;
using Moq;
using Xunit;

namespace Configurator.UnitTests.Installers
{
    public class AppConfiguratorTests : UnitTestBase<AppConfigurator>
    {
        [Fact]
        public void When_configuring_registry_settings_for_an_app()
        {
            var configuration = new AppConfiguration
            {
                RegistrySettings = new List<RegistrySetting>
                {
                    new RegistrySetting { KeyName = RandomString(), ValueName = RandomString(), ValueData = RandomString() },
                    new RegistrySetting { KeyName = RandomString(), ValueName = RandomString(), ValueData = RandomString() }
                }
            };

            var mockApp = GetMock<IApp>();
            mockApp.SetupGet(x => x.Configuration).Returns(configuration);
            var app = mockApp.Object;

            Because(() => ClassUnderTest.Configure(app));

            It("sets all provided registry settings", () =>
            {
                configuration.RegistrySettings.ForEach(setting =>
                    GetMock<IRegistryRepository>()
                        .Verify(x => x.SetValue(setting.KeyName, setting.ValueName, setting.ValueData)));
            });
        }

        [Fact]
        public void When_no_configuration_is_provided()
        {
            var mockApp = GetMock<IApp>();
            mockApp.SetupGet(x => x.Configuration).Returns((AppConfiguration)null!);
            var app = mockApp.Object;

            Because(() => ClassUnderTest.Configure(app));

            It("doesn't do anything", () =>
            {
                GetMock<IRegistryRepository>()
                    .VerifyNever(x => x.SetValue(IsAny<string>(),IsAny<string>(),IsAny<string>()));
            });
        }

        [Fact]
        public void When_a_default_configuration_is_provided()
        {
            var mockApp = GetMock<IApp>();
            mockApp.SetupGet(x => x.Configuration).Returns(new AppConfiguration());
            var app = mockApp.Object;

            Because(() => ClassUnderTest.Configure(app));

            It("doesn't do anything", () =>
            {
                GetMock<IRegistryRepository>()
                    .VerifyNever(x => x.SetValue(IsAny<string>(),IsAny<string>(),IsAny<string>()));
            });
        }

        [Fact]
        public async Task When_backing_up_an_app()
        {
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Directory = RandomString()
                }
            };
            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);
            
            var mockApp = GetMock<IApp>();
            mockApp.SetupGet(x => x.AppId).Returns(RandomString());
            mockApp.SetupGet(x => x.VerificationScript).Returns(RandomString());
            var app = mockApp.Object;
            
            GetMock<IPowerShell>().Setup(x => x.ExecuteAsync<bool>(IsAny<string>()))
                .ReturnsAsync(true);
            
            var expectedBackupScriptPath = Path.Join(settings.Manifest.Directory, $@"apps\{app.AppId}\backup.ps1");
            
            GetMock<IFileSystem>().Setup(x => x.Exists(expectedBackupScriptPath)).Returns(true);
            
            await BecauseAsync(() => ClassUnderTest.Backup(app));
            
            It("verifies the app is installed before attempting to back it up", () =>
            {
                GetMock<IPowerShell>().Verify(x => x.ExecuteAsync<bool>(app.VerificationScript!));
            });
            
            It("finds and executes the app backup script", () =>
            {
                GetMock<IPowerShell>().Verify(x => x.ExecuteAsync(expectedBackupScriptPath));
            });
        }

        [Fact]
        public async Task When_backing_up_an_app_without_verification_script()
        {
            var mockApp = GetMock<IApp>();
            mockApp.SetupGet(x => x.VerificationScript).Returns((string)null!);
            var app = mockApp.Object;
            
            await BecauseAsync(() => ClassUnderTest.Backup(app));
            
            It("does not backup non-verifiable apps", () =>
            {
                GetMock<IPowerShell>().VerifyNever(x => x.ExecuteAsync(IsAny<string>()));
            });
        }
        
        [Fact]
        public async Task When_backing_up_an_app_that_is_not_installed()
        {
            var mockApp = GetMock<IApp>();
            mockApp.SetupGet(x => x.AppId).Returns(RandomString());
            mockApp.SetupGet(x => x.VerificationScript).Returns(RandomString());
            var app = mockApp.Object;
            
            GetMock<IPowerShell>().Setup(x => x.ExecuteAsync<bool>(IsAny<string>())).ReturnsAsync(false);
            
            await BecauseAsync(() => ClassUnderTest.Backup(app));
            
            It("does not backup apps that are not installed", () =>
            {
                GetMock<IPowerShell>().VerifyNever(x => x.ExecuteAsync(IsAny<string>()));
            });
        }
        
        [Fact]
        public async Task When_backing_up_an_app_without_backup_script()
        {
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Directory = RandomString()
                }
            };
            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);
            
            var mockApp = GetMock<IApp>();
            mockApp.SetupGet(x => x.AppId).Returns(RandomString());
            mockApp.SetupGet(x => x.VerificationScript).Returns(RandomString());
            var app = mockApp.Object;
            
            GetMock<IPowerShell>().Setup(x => x.ExecuteAsync<bool>(IsAny<string>()))
                .ReturnsAsync(true);
            
            GetMock<IFileSystem>().Setup(x => x.Exists(IsAny<string>())).Returns(false);
            
            await BecauseAsync(() => ClassUnderTest.Backup(app));
            
            It("does not attempt to run non-existent backup scripts", () =>
            {
                GetMock<IPowerShell>().VerifyNever(x => x.ExecuteAsync(IsAny<string>()));
            });
        }
    }
}
