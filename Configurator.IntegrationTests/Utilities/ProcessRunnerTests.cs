using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configurator.Utilities;
using Shouldly;
using Xunit;

namespace Configurator.IntegrationTests.Utilities
{
    public class ProcessRunnerTests : IntegrationTestBase<ProcessRunner>
    {
        [Fact]
        public async Task When_executing()
        {
            var script = @"Write-Host 'Hello World'";
            var processInstructions = BuildPwshInstructions(script);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(processInstructions));

            It("runs with a successful exit code", () => { result.ExitCode.ShouldBe(0); });
        }

        [Fact]
        public async Task When_executing_and_expecting_output()
        {
            var outputs = new List<string>
            {
                RandomString(),
                RandomString(),
                RandomString()
            };

            var script = $@"Write-Output '{outputs[0]}'
Write-Output '{outputs[1]}'
Write-Output '{outputs[2]}'";
            var processInstructions = BuildPwshInstructions(script);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(processInstructions));

            It("returns the script outputs", () =>
            {
                result.AllOutput.ShouldBe(outputs);
                result.LastOutput.ShouldBe(outputs.Last());
            });
        }

        [Fact]
        public async Task When_executing_with_errors()
        {
            var errors = new List<string>
            {
                RandomString(),
                RandomString(),
                RandomString()
            };

            var expectedErrors = errors.Select(x => $"Write-Error: {x}").ToList();

            var script = $@"Write-Error '{errors[0]}'
Write-Error '{errors[1]}'
Write-Error '{errors[2]}'";
            var processInstructions = BuildPwshInstructions(script);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(processInstructions));

            It("returns the script errors", () => { result.Errors.ShouldBe(expectedErrors); });
        }
        
        private static ProcessInstructions BuildPwshInstructions(string script)
        {
            var processInstructions = new ProcessInstructions
            {
                RunAsAdmin = false,
                Executable = "pwsh.exe",
                Arguments = @$"-Command ""{script}""",
            };
            return processInstructions;
        }
    }
}
