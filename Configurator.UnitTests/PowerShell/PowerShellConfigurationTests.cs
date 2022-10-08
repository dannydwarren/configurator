using Configurator.PowerShell;
using Configurator.Utilities;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.UnitTests.PowerShell
{
    public class PowerShellConfigurationTests : UnitTestBase<PowerShellConfiguration>
    {
        [Fact]
        public async Task When_setting_execution_policy_for_powershell_core()
        {
            var getExecutionPolicyResult = RandomString();
            var getVersionResult = RandomString();

            GetMock<IPowerShell>()
                .Setup(x => x.ExecuteAsync<string>(PowerShellConfiguration.GetPolicyScript))
                .ReturnsAsync(getExecutionPolicyResult);

            GetMock<IPowerShell>()
                .Setup(x => x.ExecuteAsync<string>(PowerShellConfiguration.GetVersionScript))
                .ReturnsAsync(getVersionResult);

            await BecauseAsync(() => ClassUnderTest.SetPowerShellCoreExecutionPolicyAsync());

            It("sets and reports the policy for PowerShell Core", () =>
            {
                GetMock<IPowerShell>().Verify(x => x.ExecuteAdminAsync(PowerShellConfiguration.SetPolicyScript));
                GetMock<IConsoleLogger>().Verify(x =>
                    x.Result($"PowerShell Core - Execution Policy: {getExecutionPolicyResult}"));
            });
             
            It("gets and reports the version of Windows PowerShell", () =>
            {
                GetMock<IConsoleLogger>().Verify(x =>
                    x.Debug($"PowerShell Core - Version: {getVersionResult}"));
            });
        }

        [Fact]
        public async Task When_setting_execution_policy_for_windows_powershell()
        {
            var getExecutionPolicyResult = RandomString();
            var getVersionResult = RandomString();

            GetMock<IPowerShell>()
                .Setup(x => x.ExecuteWindowsAsync<string>(PowerShellConfiguration.GetPolicyScript))
                .ReturnsAsync(getExecutionPolicyResult);

            GetMock<IPowerShell>()
                .Setup(x => x.ExecuteWindowsAsync<string>(PowerShellConfiguration.GetVersionScript))
                .ReturnsAsync(getVersionResult);

            await BecauseAsync(() => ClassUnderTest.SetWindowsPowerShellExecutionPolicyAsync());

            It("sets and reports the policy for Windows PowerShell", () =>
            {
                GetMock<IPowerShell>().Verify(x => x.ExecuteWindowsAdminAsync(PowerShellConfiguration.SetPolicyScript));
                GetMock<IConsoleLogger>().Verify(x =>
                    x.Result($"Windows PowerShell - Execution Policy: {getExecutionPolicyResult}"));
            });
            
            It("gets and reports the version of Windows PowerShell", () =>
            {
                GetMock<IConsoleLogger>().Verify(x =>
                    x.Debug($"Windows PowerShell - Version: {getVersionResult}"));
            });
        }
    }
}
