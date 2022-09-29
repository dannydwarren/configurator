using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Installers;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Installers;

public class PowerShellCoreInstallerTests : UnitTestBase<PowerShellCoreInstaller>
{
    [Fact]
    public async Task When_installing()
    {
        IApp? capturedApp = null;
        GetMock<IAppInstallerForceWindowsPowerShell>().Setup(x => x.InstallOrUpgradeAsync(IsAny<IApp>()))
            .Callback<IApp>(app => capturedApp = app);
        
        await BecauseAsync(() => ClassUnderTest.InstallAsync());
        
        It("captures app", () => capturedApp.ShouldBe(PowerShellCoreInstaller.PowerShellApp));
    }
}
