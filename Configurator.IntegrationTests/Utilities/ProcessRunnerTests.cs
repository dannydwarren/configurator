using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configurator.Configuration;
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
        public async Task When_executing_returns_json()
        {
            var var1 = RandomString();
            var var2 = new Random().Next();

            var script = $@"
$var1Val = '{var1}'
$var2Val = {var2}
Write-Output """"""{{ """"""""Var1"""""""": """"""""$var1Val"""""""", """"""""Var2"""""""": $var2Val }}""""""";
            var processInstructions = BuildPwshInstructions(script);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(processInstructions));

            It("runs with a successful exit code", () => { result.ExitCode.ShouldBe(0); });
            
            It("should be valid JSON", () =>
            {
                result.LastOutput.ShouldNotBeNull();
                var deserializedResult = new JsonSerializer().Deserialize<JsonTestResult>(result.LastOutput!);
                deserializedResult.ShouldNotBeNull();
                deserializedResult.Var1.ShouldBe(var1);
                deserializedResult.Var2.ShouldBe(var2);
            });
        }

        public class JsonTestResult
        {
            public string Var1 { get; set; } = "";
            public int Var2 { get; set; } = 0;
        }
        
        [Fact]
        public async Task When_executing_as_admin()
        {
            var script = @"$result = [bool](([System.Security.Principal.WindowsIdentity]::GetCurrent()).groups -match 'S-1-5-32-544')
Write-Output $result";
            var processInstructions = BuildPwshInstructions(script, runAsAdmin: true);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(processInstructions));

            It("only exit code can, therefore should, be populated indicating success", () =>
            {
                result.ExitCode.ShouldBe(0);
                result.LastOutput.ShouldBeNull();
                result.AllOutput.ShouldBeEmpty();
                result.Errors.ShouldBeEmpty();
            });
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
        
        private static ProcessInstructions BuildPwshInstructions(string script, bool runAsAdmin = false)
        {
            var processInstructions = new ProcessInstructions
            {
                RunAsAdmin = runAsAdmin,
                Executable = "pwsh.exe",
                Arguments = $"-Command {script}"
            };
            return processInstructions;
        }
    }
}
