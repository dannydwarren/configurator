using System.Threading.Tasks;
using Configurator.Installers;
using Configurator.PowerShell;
using Xunit;

namespace Configurator.UnitTests.Installers
{
    public class WingetConfigurationTests : UnitTestBase<WingetConfiguration>
    {
        [Fact]
        public async Task When_upgrading_to_latest_version()
        {
            await BecauseAsync(() => ClassUnderTest.UpgradeAsync());

            It("installs and updates sources", () =>
            {
                //https://github.com/microsoft/winget-cli/issues/3652#issuecomment-1796306100
                GetMock<IPowerShell>().Verify(x => x.ExecuteWindowsAsync("Add-AppxPackage https://github.com/microsoft/winget-cli/releases/latest/download/Microsoft.DesktopAppInstaller_8wekyb3d8bbwe.msixbundle -ForceTargetApplicationShutdown"));
                GetMock<IPowerShell>().Verify(x => x.ExecuteWindowsAsync("Add-AppxPackage https://cdn.winget.microsoft.com/cache/source.msix"));
            });
        }

        [Fact]
        public async Task When_accepting_source_agreements()
        {
            await BecauseAsync(() => ClassUnderTest.AcceptSourceAgreementsAsync());

            It("accepts all source agreements", () =>
            {
                GetMock<IPowerShell>().Verify(x => x.ExecuteWindowsAsync("winget list winget --accept-source-agreements"));
            });
        }
    }
}
