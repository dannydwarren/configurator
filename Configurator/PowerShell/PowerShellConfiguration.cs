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
        private readonly IConsoleLogger consoleLogger;

        public PowerShellConfiguration(IPowerShell powerShell, IConsoleLogger consoleLogger)
        {
            this.powerShell = powerShell;
            this.consoleLogger = consoleLogger;
        }

        public async Task SetExecutionPolicyAsync()
        {
            var setScript = "Set-ExecutionPolicy RemoteSigned -Force";
            var getScript = "Get-ExecutionPolicy";

            await SetPowerShellCoreExecutionPolicy(setScript, getScript);
            await SetWindowsPowerShellExecutionPolicy(setScript, getScript);
        }

        private async Task SetPowerShellCoreExecutionPolicy(string setScript, string getScript)
        {
            await powerShell.ExecuteAdminAsync(setScript);
            var result = await powerShell.ExecuteAsync<string>(getScript);
            consoleLogger.Result($"PowerShell - Execution Policy: {result}");
        }

        private async Task SetWindowsPowerShellExecutionPolicy(string setScript, string getScript)
        {
            await powerShell.ExecuteWindowsAdminAsync(setScript);
            var result = await powerShell.ExecuteWindowsAsync<string>(getScript);
            consoleLogger.Result($"Windows PowerShell - Execution Policy: {result}");
        }
    }
}
