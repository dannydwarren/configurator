using System.Threading.Tasks;
using Configurator.Installers;
using Configurator.PowerShell;
using Xunit;

namespace Configurator.UnitTests
{
    public class SystemInitializerTests : UnitTestBase<SystemInitializer>
    {
        [Fact]
        public async Task When_initializing_the_system()
        {
            await BecauseAsync(() => ClassUnderTest.InitializeAsync());

            It("sets the Windows PowerShell execution policy", () =>
            {
                GetMock<IPowerShellConfiguration>().Verify(x => x.SetWindowsPowerShellExecutionPolicyAsync());
            });

            It("installs PowerShell Core", () =>
            {
                GetMock<IPowerShellInstaller>().Verify(x => x.InstallAsync());
            });

            It("sets the PowerShell Core execution policy", () =>
            {
                GetMock<IPowerShellConfiguration>().Verify(x => x.SetPowerShellCoreExecutionPolicyAsync());
            });

            It("installs winget-cli", () =>
            {
                GetMock<IWingetCliInstaller>().Verify(x => x.InstallAsync());
            });

            It("installs scoop-cli", () =>
            {
                GetMock<IScoopCliInstaller>().Verify(x => x.InstallAsync());
            });
            
            It("installs git", () =>
            {
                GetMock<IGitInstaller>().Verify(x => x.InstallAsync());
            });
        }
    }
}
