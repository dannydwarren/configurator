using System.Threading.Tasks;
using Configurator.Installers;
using Xunit;

namespace Configurator.UnitTests.Installers;

public class PowerShellCoreInstallerTests : UnitTestBase<PowerShellCoreInstaller>
{
    [Fact]
    public async Task When_installing()
    {
        await BecauseAsync(() => ClassUnderTest.InstallAsync());

        It("installs", () =>
            GetMock<IAppInstallerForceWindowsPowerShell>()
                .Verify(x => x.InstallOrUpgradeAsync(PowerShellCoreInstaller.PowerShellApp))
        );
    }
}
