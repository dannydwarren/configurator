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
        public const string SetPolicyScript = "Set-ExecutionPolicy RemoteSigned -Force";
        public const string GetPolicyScript = "Get-ExecutionPolicy";
        public const string GetVersionScript = "$PSVersionTable.PSVersion.ToString()";
        private readonly IPowerShell powerShell;
        private readonly IConsoleLogger consoleLogger;
        
        public PowerShellConfiguration(IPowerShell powerShell, IConsoleLogger consoleLogger)
        {
            this.powerShell = powerShell;
            this.consoleLogger = consoleLogger;
        }

        public async Task SetPowerShellCoreExecutionPolicyAsync()
        {
            await powerShell.ExecuteAdminAsync(SetPolicyScript);
            var policyResult = await powerShell.ExecuteAsync<string>(GetPolicyScript);
            consoleLogger.Result($"PowerShell Core - Execution Policy: {policyResult}");
            
            var versionResult = await powerShell.ExecuteAsync<string>(GetVersionScript);
            consoleLogger.Debug($"PowerShell Core - Version: {versionResult}");
        }
      
        public async Task SetWindowsPowerShellExecutionPolicyAsync()
        {
            await powerShell.ExecuteWindowsAdminAsync(SetPolicyScript);
            var policyResult = await powerShell.ExecuteWindowsAsync<string>(GetPolicyScript);
            consoleLogger.Result($"Windows PowerShell - Execution Policy: {policyResult}");
            
            var versionResult = await powerShell.ExecuteWindowsAsync<string>(GetVersionScript);
            consoleLogger.Debug($"Windows PowerShell - Version: {versionResult}");
        }
    }
}
