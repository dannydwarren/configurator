using Configurator.Utilities;
using System.Threading.Tasks;

namespace Configurator.PowerShell
{
    public interface IPowerShellConfiguration
    {
        Task SetExecutionPolicyAsync();
    }

    public class PowerShellConfiguration : IPowerShellConfiguration
    {
        private readonly IPowerShell powerShell;
        private readonly IWindowsPowerShell windowsPowerShell;
        private readonly IConsoleLogger consoleLogger;

        public PowerShellConfiguration(IPowerShell powerShell, IWindowsPowerShell windowsPowerShell, IConsoleLogger consoleLogger)
        {
            this.powerShell = powerShell;
            this.windowsPowerShell = windowsPowerShell;
            this.consoleLogger = consoleLogger;
        }

        public async Task SetExecutionPolicyAsync()
        {
            var executionPolicy = "RemoteSigned";
            var setScript = @$"Set-ExecutionPolicy {executionPolicy} -Force";
            var getScript = "Get-ExecutionPolicy";

            await powerShell.ExecuteAsync(setScript, runAsAdmin: true);
            var getScriptResult = await powerShell.ExecuteAsync<string>(getScript);
            consoleLogger.Result($"PowerShell - Execution Policy: {getScriptResult}");

            windowsPowerShell.Execute(setScript);
            consoleLogger.Result($"Windows PowerShell - Execution Policy: {executionPolicy}");
        }
    }
}
