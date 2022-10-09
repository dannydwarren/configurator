using System.Threading.Tasks;
using Configurator.Installers;
using Configurator.PowerShell;
using Xunit;

namespace Configurator.UnitTests.Installers
{
    public class WingetConfigurationTests : UnitTestBase<WingetConfiguration>
    {
        [Fact]
        public async Task When_installing_winget_cli()
        {
            await BecauseAsync(() => ClassUnderTest.AcceptSourceAgreementsAsync());

            It("accepts all source agreements", () =>
            {
                GetMock<IPowerShell>().Verify(x => x.ExecuteWindowsAsync("winget list winget --accept-source-agreements"));
            });
        }
    }
}
