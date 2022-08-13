using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Configurator.IntegrationTests.PowerShell
{
    public class PowerShellTests : IntegrationTestBase<Configurator.PowerShell.PowerShell>
    {
        [Fact]
        public async Task When_executing()
        {
            var script = @"Write-Host 'Hello World'";

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script));

            It("runs with a successful exit code", () =>
            {
                result.ExitCode.ShouldBe(0);
            });
        }
        
        [Fact]
        public async Task When_executing_and_expecting_result()
        {
            var expectedOutput = RandomString();
            
            var script = $@"Write-Output '{expectedOutput}'";

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script));

            It("returns the script output", () =>
            {
                result.Output.ShouldNotBeNull().ShouldBe(expectedOutput);
            });
        }
    }
}
