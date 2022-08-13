using System;
using System.Threading.Tasks;
using Configurator.Utilities;

namespace Configurator.PowerShell
{
    public interface IPowerShell
    {
        Task ExecuteAsync(string script);
        Task<TResult> ExecuteAsync<TResult>(string script);
    }

    public class PowerShell : IPowerShell
    {
        private readonly IPowerShellRunner powerShellRunner;
        private readonly IConsoleLogger consoleLogger;

        public PowerShell(IPowerShellRunner powerShellRunner, IConsoleLogger consoleLogger)
        {
            this.powerShellRunner = powerShellRunner;
            this.consoleLogger = consoleLogger;
        }

        public async Task ExecuteAsync(string script)
        {
            await InternalExecuteAsync(script);
        }

        public async Task<TResult> ExecuteAsync<TResult>(string script)
        {
            var result = await InternalExecuteAsync(script);

            return Map<TResult>(result.LastOutput);
        }

        private async Task<PowerShellResult> InternalExecuteAsync(string script)
        {
            var result = await powerShellRunner.ExecuteAsync(script);

            if (result.ExitCode != 0)
                throw new Exception($"Script failed to complete with exit code {result.ExitCode}");
            
            result.Errors.ForEach(consoleLogger.Error);

            return result;
        }

        private TResult Map<TResult>(string? output)
        {
            if (output == null)
                return default;

            var resultType = typeof(TResult);
            object objResult = resultType switch
            {
                {} when resultType == typeof(string) => output,
                {} when resultType == typeof(bool) => bool.Parse(output),
                _ => throw new NotSupportedException($"PowerShell result type of '{typeof(TResult).FullName}' is not yet supported")
            };

            return (TResult)objResult;
        }
    }
}
