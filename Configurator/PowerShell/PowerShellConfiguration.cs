using Configurator.Utilities;
using System.Threading.Tasks;

namespace Configurator.PowerShell
{
    public interface IPowerShellConfiguration
    {
        Task SetPowerShellCoreExecutionPolicyAsync();
        Task SetWindowsPowerShellExecutionPolicyAsync();
    }

    public class PowerShellConfiguration : IPowerShellConfiguration
    {
        private const string SetScript = "Set-ExecutionPolicy RemoteSigned -Force";
        private const string GetScript = "Get-ExecutionPolicy";
        private readonly IPowerShell powerShell;
        private readonly IConsoleLogger consoleLogger;
        
        public PowerShellConfiguration(IPowerShell powerShell, IConsoleLogger consoleLogger)
        {
            this.powerShell = powerShell;
            this.consoleLogger = consoleLogger;
        }

        public async Task SetPowerShellCoreExecutionPolicyAsync()
        {
            await powerShell.ExecuteAdminAsync(SetScript);
            var result = await powerShell.ExecuteAsync<string>(GetScript);
            consoleLogger.Result($"PowerShell Core - Execution Policy: {result}");
        }
      
        public async Task SetWindowsPowerShellExecutionPolicyAsync()
        {
            await powerShell.ExecuteWindowsAdminAsync(SetScript);
            var result = await powerShell.ExecuteWindowsAsync<string>(GetScript);
            consoleLogger.Result($"Windows PowerShell - Execution Policy: {result}");
        }
    }
}
