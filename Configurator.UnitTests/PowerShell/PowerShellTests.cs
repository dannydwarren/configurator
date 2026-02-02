using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Configurator.Utilities;
using Configurator.Windows;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests.PowerShell
{
    public class PowerShellTests : UnitTestBase<Configurator.PowerShell.PowerShell>
    {
        private readonly string myDocumentsPath;

        public PowerShellTests()
        {
            myDocumentsPath = RandomString();
            GetMock<ISpecialFolders>().Setup(x => x.GetMyDocumentsPath()).Returns(myDocumentsPath);
        }
        
        [Fact]
        public async Task When_executing_core()
        {
            var powerShellCoreInstallLocation = RandomString();
            var script = RandomString();
            var currentUserCurrentHostProfile = Path.Combine(myDocumentsPath, "PowerShell\\Microsoft.PowerShell_profile.ps1");
            var environmentReadyScript = $@"
$env:Path = [System.Environment]::GetEnvironmentVariable(""Path"",""Machine"") + "";"" + [System.Environment]::GetEnvironmentVariable(""Path"",""User"")

if ($profile -eq $null -or $profile -eq '') {{
  $global:profile = ""{currentUserCurrentHostProfile}""
}}

{script}";
            var scriptFilePath = RandomString();

            var powerShellResult = new ProcessResult
            {
                ExitCode = 0
            };
            
            ProcessInstructions? capturedInstructions = null;
            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .Callback<ProcessInstructions>(instructions => capturedInstructions = instructions)
                .ReturnsAsync(powerShellResult);

            var installedVersionGuid = Guid.NewGuid().ToString();
            GetMock<IRegistryRepository>().Setup(x => x.GetSubKeyNames(@"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions"))
                .Returns(new[] { installedVersionGuid });
            GetMock<IRegistryRepository>().Setup(x => x.GetValue(
                    $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{installedVersionGuid}",
                    "InstallLocation"))
                .Returns(powerShellCoreInstallLocation);

            GetMock<IScriptToFileConverter>().Setup(x => x.ToPowerShellAsync(environmentReadyScript)).ReturnsAsync(scriptFilePath);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script));

            It("runs the script", () =>
            {
                capturedInstructions.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.RunAsAdmin.ShouldBeFalse();
                    x.Executable.ShouldBe($"{powerShellCoreInstallLocation}pwsh.exe");
                    x.Arguments.ShouldBe($"-File {scriptFilePath}");
                });
            });
        }

        [Fact]
        public async Task When_executing_windows()
        {
            var script = RandomString();
            var currentUserCurrentHostProfile = Path.Combine(myDocumentsPath, "WindowsPowerShell\\Microsoft.PowerShell_profile.ps1");
            var environmentReadyScript = $@"
$env:Path = [System.Environment]::GetEnvironmentVariable(""Path"",""Machine"") + "";"" + [System.Environment]::GetEnvironmentVariable(""Path"",""User"")

if ($profile -eq $null -or $profile -eq '') {{
  $global:profile = ""{currentUserCurrentHostProfile}""
}}

{script}";  var scriptFilePath = RandomString();

            var powerShellResult = new ProcessResult
            {
                ExitCode = 0
            };

            ProcessInstructions? capturedInstructions = null;
            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .Callback<ProcessInstructions>(instructions => capturedInstructions = instructions)
                .ReturnsAsync(powerShellResult);

            GetMock<IScriptToFileConverter>().Setup(x => x.ToPowerShellAsync(environmentReadyScript)).ReturnsAsync(scriptFilePath);

            await BecauseAsync(() => ClassUnderTest.ExecuteWindowsAsync(script));

            It("runs the script", () =>
            {
                capturedInstructions.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.RunAsAdmin.ShouldBeFalse();
                    x.Executable.ShouldBe("powershell.exe");
                    x.Arguments.ShouldBe($@"-File {scriptFilePath}");
                });
            });
        }

        [Theory]
        [InlineData(PowerShellVersion.Core, "pwsh.exe")]
        [InlineData(PowerShellVersion.Windows, "powershell.exe")]
        public async Task When_executing_as_admin(PowerShellVersion versionUnderTest, string expectedExecutable)
        {
            var script = RandomString();
            var scriptFilePath = RandomString();

            var powerShellResult = new ProcessResult
            {
                ExitCode = 0
            };

            ProcessInstructions? capturedInstructions = null;
            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .Callback<ProcessInstructions>(instructions => capturedInstructions = instructions)
                .ReturnsAsync(powerShellResult);

            GetMock<IScriptToFileConverter>().Setup(x => x.ToPowerShellAsync(IsAny<string>())).ReturnsAsync(scriptFilePath);

            var installedVersionGuid = Guid.NewGuid().ToString();
            GetMock<IRegistryRepository>().Setup(x => x.GetSubKeyNames(@"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions"))
                .Returns(new[] { installedVersionGuid });
            GetMock<IRegistryRepository>().Setup(x => x.GetValue(
                    $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{installedVersionGuid}",
                    "InstallLocation"))
                .Returns("");

            if (versionUnderTest == PowerShellVersion.Core)
                await BecauseAsync(() => ClassUnderTest.ExecuteAdminAsync(script));
            else if (versionUnderTest == PowerShellVersion.Windows)
                await BecauseAsync(() => ClassUnderTest.ExecuteWindowsAdminAsync(script));

            It("runs the script", () =>
            {
                capturedInstructions.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.RunAsAdmin.ShouldBeTrue();
                    x.Executable.ShouldBe(expectedExecutable);
                    x.Arguments.ShouldBe($@"-File {scriptFilePath}");
                });
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

            var installedVersionGuid = Guid.NewGuid().ToString();
            GetMock<IRegistryRepository>().Setup(x => x.GetSubKeyNames(@"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions"))
                .Returns(new[] { installedVersionGuid });
            GetMock<IRegistryRepository>().Setup(x => x.GetValue(
                    $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{installedVersionGuid}",
                    "InstallLocation"))
                .Returns("");

            var exception = await BecauseThrowsAsync<Exception>(() => ClassUnderTest.ExecuteAsync(script));

            It("throws", () => exception.ShouldNotBeNull());
        }

        [Fact]
        public async Task When_executing_with_errors()
        {
            var script = RandomString();
            var powerShellResult = new ProcessResult
            {
                ExitCode = -1,
                Errors = new List<string>
                {
                    RandomString(),
                    RandomString(),
                    RandomString()
                }
            };

            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .ReturnsAsync(powerShellResult);

            var installedVersionGuid = Guid.NewGuid().ToString();
            GetMock<IRegistryRepository>().Setup(x => x.GetSubKeyNames(@"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions"))
                .Returns(new[] { installedVersionGuid });
            GetMock<IRegistryRepository>().Setup(x => x.GetValue(
                    $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{installedVersionGuid}",
                    "InstallLocation"))
                .Returns("");

            var exception = await BecauseThrowsAsync<Exception>(() => ClassUnderTest.ExecuteAsync(script));

            It("throws to indicate failed execution", () =>
                exception.ShouldNotBeNull()
                    .Message.ShouldBe($"Script failed to complete with exit code {powerShellResult.ExitCode}"));
            
            It("logs the errors to the console",
                () => { GetMock<IConsoleLogger>().Verify(x => x.Error(IsAny<string>()), Times.Exactly(3)); });
        }


        [Theory]
        [InlineData(PowerShellVersion.Core, "pwsh.exe")]
        [InlineData(PowerShellVersion.Windows, "powershell.exe")]
        public async Task When_executing_and_expecting_a_result_type(PowerShellVersion versionUnderTest,
            string expectedExecutable)
        {
            var script = RandomString();
            var scriptFilePath = RandomString();
            var powerShellResult = new ProcessResult
            {
                ExitCode = 0,
                LastOutput = RandomString()
            };

            ProcessInstructions? capturedInstructions = null;
            GetMock<IProcessRunner>().Setup(x => x.ExecuteAsync(IsAny<ProcessInstructions>()))
                .Callback<ProcessInstructions>(instructions => capturedInstructions = instructions)
                .ReturnsAsync(powerShellResult);

            GetMock<IScriptToFileConverter>().Setup(x => x.ToPowerShellAsync(IsAny<string>())).ReturnsAsync(scriptFilePath);

            var installedVersionGuid = Guid.NewGuid().ToString();
            GetMock<IRegistryRepository>().Setup(x => x.GetSubKeyNames(@"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions"))
                .Returns(new[] { installedVersionGuid });
            GetMock<IRegistryRepository>().Setup(x => x.GetValue(
                    $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{installedVersionGuid}",
                    "InstallLocation"))
                .Returns("");

            var result = versionUnderTest switch
            {
                PowerShellVersion.Core => await BecauseAsync(() => ClassUnderTest.ExecuteAsync<string>(script)),
                PowerShellVersion.Windows => await BecauseAsync(
                    () => ClassUnderTest.ExecuteWindowsAsync<string>(script)),
                _ => throw new ArgumentOutOfRangeException(nameof(versionUnderTest), versionUnderTest, null)
            };

            It("runs the script", () =>
            {
                capturedInstructions.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.RunAsAdmin.ShouldBeFalse();
                    x.Executable.ShouldBe(expectedExecutable);
                    x.Arguments.ShouldBe($@"-File {scriptFilePath}");
                });
            });

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

            var installedVersionGuid = Guid.NewGuid().ToString();
            GetMock<IRegistryRepository>().Setup(x => x.GetSubKeyNames(@"SOFTWARE\Microsoft\PowerShellCore\InstalledVersions"))
                .Returns(new[] { installedVersionGuid });
            GetMock<IRegistryRepository>().Setup(x => x.GetValue(
                    $@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\{installedVersionGuid}",
                    "InstallLocation"))
                .Returns("");

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
