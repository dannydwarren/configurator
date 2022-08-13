using System;
using System.Threading.Tasks;
using Configurator.PowerShell;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.PowerShell
{
    public class PowerShellTests : UnitTestBase<Configurator.PowerShell.PowerShell>
    {
        [Fact]
        public async Task When_executing()
        {
            var script = RandomString();
            var powerShellResult = new PowerShellResult
            {
                ExitCode = 0
            };

            GetMock<IPowerShellRunner>().Setup(x => x.ExecuteAsync(script)).ReturnsAsync(powerShellResult);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script));

            It("runs the script", () => { GetMock<IPowerShellRunner>().Verify(x => x.ExecuteAsync(script)); });
        }

        [Fact]
        public async Task When_executing_with_unsuccessful_exit_code()
        {
            var script = RandomString();
            var powerShellResult = new PowerShellResult
            {
                ExitCode = 1
            };

            GetMock<IPowerShellRunner>().Setup(x => x.ExecuteAsync(script)).ReturnsAsync(powerShellResult);

            var exception = await BecauseThrowsAsync<Exception>(() => ClassUnderTest.ExecuteAsync(script));

            It("throws", () => exception.ShouldNotBeNull());
        }

        [Fact]
        public async Task When_executing_and_expecting_a_result_type_of_string()
        {
            var script = RandomString();
            var powerShellResult = new PowerShellResult
            {
                ExitCode = 0,
                LastOutput = RandomString()
            };

            GetMock<IPowerShellRunner>().Setup(x => x.ExecuteAsync(script)).ReturnsAsync(powerShellResult);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync<string>(script));

            It("returns a typed result", () => { result.ShouldBe(powerShellResult.LastOutput); });
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("TRUE", true)]
        [InlineData("false", false)]
        [InlineData("False", false)]
        [InlineData("FALSE", false)]
        public async Task When_executing_and_expecting_a_result_type_of_bool(string lastOutput, bool expectedResult)
        {
            var script = RandomString();
            var powerShellResult = new PowerShellResult
            {
                ExitCode = 0,
                LastOutput = lastOutput
            };

            GetMock<IPowerShellRunner>().Setup(x => x.ExecuteAsync(script)).ReturnsAsync(powerShellResult);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync<bool>(script));

            It("returns a typed result", () => { result.ShouldBe(expectedResult); });
        }
    }
}
