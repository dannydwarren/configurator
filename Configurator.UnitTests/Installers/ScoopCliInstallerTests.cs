using System.Threading.Tasks;
using Configurator.Installers;
using Xunit;

namespace Configurator.UnitTests.Installers
{
    public class ScoopCliInstallerTests : UnitTestBase<ScoopCliInstaller>
    {
        [Fact]
        public async Task When_installing_scoop_cli()
        {
            await BecauseAsync(() => ClassUnderTest.InstallAsync());

            It("installs", () =>
                GetMock<IAppInstaller>().Verify(x => x.InstallOrUpgradeAsync(ScoopCliInstaller.ScoopCliScriptApp))
            );
        }
    }
}
