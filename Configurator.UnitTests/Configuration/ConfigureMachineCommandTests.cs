using Configurator.Apps;
using Configurator.Configuration;
using Configurator.Installers;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.UnitTests.Configuration
{
    public class ConfigureMachineCommandTests : UnitTestBase<ConfigureMachineCommand>
    {
        [Fact]
        public async Task When_installing_all_manifest_apps()
        {
            var manifest = new Manifest_V2
            {
                Apps =
                {
                    new ScriptApp { AppId = RandomString() },
                    new ScriptApp { AppId = RandomString() },
                    new PowerShellAppPackage { AppId = RandomString() }
                }
            };

            var manifestRepositoryMock = GetMock<IManifestRepository_V2>();
            manifestRepositoryMock.Setup(x => x.LoadAsync()).ReturnsAsync(manifest);

            var appInstallerMock = GetMock<IAppInstaller>();
            var downloadAppInstallerMock = GetMock<IDownloadAppInstaller>();
            var appConfiguratorMock = GetMock<IAppConfigurator>();

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync());

            It("installs each app", () =>
            {
                appInstallerMock.Verify(x => x.InstallOrUpgradeAsync(manifest.Apps[0]));
                appInstallerMock.Verify(x => x.InstallOrUpgradeAsync(manifest.Apps[1]));
                downloadAppInstallerMock.Verify(x => x.InstallAsync((IDownloadApp)manifest.Apps[2]));
            });

            It("configures each app", () =>
            {
                appConfiguratorMock.Verify(x => x.Configure(manifest.Apps[0]));
                appConfiguratorMock.Verify(x => x.Configure(manifest.Apps[1]));
                appConfiguratorMock.Verify(x => x.Configure(manifest.Apps[2]));
            });
        }
    }
}
