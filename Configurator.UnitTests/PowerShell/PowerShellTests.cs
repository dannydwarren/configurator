using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Utilities;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.PowerShell
{
    public class PowerShellTests : UnitTestBase<Configurator.PowerShell.PowerShell>
    {
        [Fact]
        public async Task When_executing_core()
        {
            var script = RandomString();

            var powerShellResult = new ProcessResult
            {
                ExitCode = 0
            };

            ProcessInstructions? capturedInstructions = null;
            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .Callback<ProcessInstructions>(instructions => capturedInstructions = instructions)
                .ReturnsAsync(powerShellResult);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script));

            It("runs the script", () =>
            {
                capturedInstructions.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                    {
                        x.RunAsAdmin.ShouldBeFalse();
                        x.Executable.ShouldBe("pwsh.exe");
                        x.Arguments.ShouldBe($"-Command {script}");
                    });
            });
        }

        [Fact]
        public async Task When_executing_windows()
        {
            var script = RandomString();

            var powerShellResult = new ProcessResult
            {
                ExitCode = 0
            };

            ProcessInstructions? capturedInstructions = null;
            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .Callback<ProcessInstructions>(instructions => capturedInstructions = instructions)
                .ReturnsAsync(powerShellResult);

            await BecauseAsync(() => ClassUnderTest.ExecuteWindowsAsync(script));

            It("runs the script", () =>
            {
                capturedInstructions.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                    {
                        x.RunAsAdmin.ShouldBeFalse();
                        x.Executable.ShouldBe("powershell.exe");
                        x.Arguments.ShouldBe($@"""{script}""");
                    });
            });
        }

        [Theory]
        [InlineData(PowerShellVersion.Core)]
        [InlineData(PowerShellVersion.Windows)]
        public async Task When_executing_as_admin(PowerShellVersion versionUnderTest)
        {
            var script = RandomString();

            var powerShellResult = new ProcessResult
            {
                ExitCode = 0
            };

            ProcessInstructions? capturedInstructions = null;
            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .Callback<ProcessInstructions>(instructions => capturedInstructions = instructions)
                .ReturnsAsync(powerShellResult);

            if (versionUnderTest == PowerShellVersion.Core)
                await BecauseAsync(() => ClassUnderTest.ExecuteAdminAsync(script));
            else if (versionUnderTest == PowerShellVersion.Windows)
                await BecauseAsync(() => ClassUnderTest.ExecuteWindowsAdminAsync(script));

            It("runs the script", () =>
            {
                capturedInstructions.ShouldNotBeNull().RunAsAdmin.ShouldBeTrue();
            });
        }
        
        [Fact]
        public async Task When_executing_with_unsuccessful_exit_code()
        {
            var script = RandomString();
            var powerShellResult = new ProcessResult
            {
                ExitCode = 1
            };

            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .ReturnsAsync(powerShellResult);

            var exception = await BecauseThrowsAsync<Exception>(() => ClassUnderTest.ExecuteAsync(script));

            It("throws", () => exception.ShouldNotBeNull());
        }

        [Fact]
        public async Task When_executing_with_errors()
        {
            var script = RandomString();
            var powerShellResult = new ProcessResult
            {
                ExitCode = 0,
                Errors = new List<string>
                {
                    RandomString(),
                    RandomString(),
                    RandomString()
                }
            };

            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .ReturnsAsync(powerShellResult);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script));

            It("logs the errors to the console",
                () => { GetMock<IConsoleLogger>().Verify(x => x.Error(IsAny<string>()), Times.Exactly(3)); });
        }


        [Theory]
        [InlineData(PowerShellVersion.Core)]
        [InlineData(PowerShellVersion.Windows)]
        public async Task When_executing_and_expecting_a_result_type(PowerShellVersion versionUnderTest)
        {
            var script = RandomString();
            var powerShellResult = new ProcessResult
            {
                ExitCode = 0,
                LastOutput = RandomString()
            };

            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .ReturnsAsync(powerShellResult);

            var result = versionUnderTest switch
            {
                PowerShellVersion.Core => await BecauseAsync(() => ClassUnderTest.ExecuteAsync<string>(script)),
                PowerShellVersion.Windows => await BecauseAsync(() => ClassUnderTest.ExecuteWindowsAsync<string>(script)),
                _ => throw new ArgumentOutOfRangeException(nameof(versionUnderTest), versionUnderTest, null)
            };
            
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
            var powerShellResult = new ProcessResult
            {
                ExitCode = 0,
                LastOutput = lastOutput
            };

            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .ReturnsAsync(powerShellResult);

            var result = await BecauseAsync(() => ClassUnderTest.ExecuteAsync<bool>(script));

            It("returns a typed result", () => { result.ShouldBe(expectedResult); });
        }

        public enum PowerShellVersion
        {
            Core,
            Windows
        }
    }
}
