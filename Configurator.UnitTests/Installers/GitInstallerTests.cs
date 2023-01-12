using System.Threading.Tasks;
using Configurator.Installers;
using Xunit;

namespace Configurator.UnitTests.Installers
{
    public class GitInstallerTests : UnitTestBase<GitInstaller>
    {
        [Fact]
        public async Task When_installing()
        {
            await BecauseAsync(() => ClassUnderTest.InstallAsync());

            It("installs", () =>
                GetMock<IAppInstaller>().Verify(x => x.InstallOrUpgradeAsync(GitInstaller.GitWingetApp))
            );
        }
    }
}
