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
        public async Task When_setting_execution_policy()
        {
            var executionPolicy = "RemoteSigned";
            var getExecutionPolicyResult = RandomString();

            GetMock<IPowerShell>()
                .Setup(x => x.ExecuteAsync<string>("Get-ExecutionPolicy"))
                .ReturnsAsync(getExecutionPolicyResult);

            await BecauseAsync(() => ClassUnderTest.SetExecutionPolicyAsync());

            It("reports the set policy for PowerShell",
                () =>
                {
                    GetMock<IPowerShell>().Verify(x => x.ExecuteAsync(Is<string>(y => y.Contains(executionPolicy)), true));
                    GetMock<IConsoleLogger>().Verify(x =>
                        x.Result($"PowerShell - Execution Policy: {getExecutionPolicyResult}"));
                });

            It("sets and reports the set policy for Windows PowerShell", () =>
            {
                GetMock<IWindowsPowerShell>().Verify(x => x.Execute(Is<string>(y => y.Contains(executionPolicy))));
                GetMock<IConsoleLogger>()
                    .Verify(x => x.Result($"Windows PowerShell - Execution Policy: {executionPolicy}"));
            });
        }
    }
}
