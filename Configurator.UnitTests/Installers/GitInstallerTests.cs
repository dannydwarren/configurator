using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.Installers;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.Installers
{
    public class GitInstallerTests : UnitTestBase<GitInstaller>
    {
        [Fact]
        public async Task When_installing_git()
        {
            IApp capturedApp = null!;
            GetMock<IAppInstaller>().Setup(x => x.InstallOrUpgradeAsync(IsAny<IApp>()))
                .Callback<IApp>(app => capturedApp = app);

            await BecauseAsync(() => ClassUnderTest.InstallAsync());

            It("installs", () => { capturedApp.ShouldBe(GitInstaller.GitWingetApp); });
        }
    }
}
