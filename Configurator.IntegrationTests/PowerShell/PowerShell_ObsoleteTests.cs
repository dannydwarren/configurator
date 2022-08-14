using AutoMoqCore;
using Configurator.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.IntegrationTests.PowerShell
{
    public class PowerShell_ObsoleteTests : IntegrationTestBase<Configurator.PowerShell.PowerShell_Obsolete>
    {
        private readonly Mock<IConsoleLogger> mockConsoleLogger;

        public PowerShell_ObsoleteTests()
        {
            AutoMoqer mocker = new AutoMoqer();
            mockConsoleLogger = mocker.GetMock<IConsoleLogger>();

            Services.AddTransient(_ => mockConsoleLogger.Object);
        }

        [Fact]
        public async Task When_executing_script_for_the_first_time_with_complete_check()
        {
            var script = @"$testVar = $false
Write-Information ""testVar=$testVar""
$testVar = $true
Write-Information ""testVar=$testVar""";
            var completeCheckScript = "$testVar -eq $true";

            var output = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script, completeCheckScript));

            It("returns output", () =>
            {
                mockConsoleLogger.Verify(x => x.Info("testVar=False"));
                mockConsoleLogger.Verify(x => x.Info("testVar=True"));
                output.AsBool.ShouldNotBeNull().ShouldBeTrue();
            });
        }

        [Fact]
        public async Task When_executing_script_that_has_already_been_completed()
        {
            var script = @"Write-Information 'Should not get this'";
            var completeCheckScript = "$true";

            var output = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script, completeCheckScript));

            It("returns output", () =>
            {
                mockConsoleLogger.Verify(x => x.Info("Should not get this"), Times.Exactly(0));
                output.AsBool.ShouldNotBeNull().ShouldBeTrue();
            });
        }

        [Fact]
        public async Task When_executing_script_for_the_first_time_with_failing_complete_check()
        {
            var script = @"Write-Information 'Script run'";
            var completeCheckScript = "$false";

            var output = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script, completeCheckScript));

            It("returns output", () =>
            {
                mockConsoleLogger.Verify(x => x.Info("Script run"));
                output.AsBool.ShouldNotBeNull().ShouldBeFalse();
            });
        }

        [Fact]
        public async Task When_executing_script_for_the_first_time_with_complete_check_missing_result()
        {
            var script = @"Write-Information 'Script run'";
            var completeCheckScript = "'NotTrue'";

            var output = await BecauseAsync(() => ClassUnderTest.ExecuteAsync(script, completeCheckScript));

            It("returns output", () =>
            {
                mockConsoleLogger.Verify(x => x.Info("Script run"));
                output.AsString.ShouldBe("NotTrue");
                output.AsBool.ShouldBeNull();
            });
        }
    }
}
