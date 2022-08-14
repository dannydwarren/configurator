using System;
using System.Threading.Tasks;
using Configurator.Utilities;

namespace Configurator.PowerShell
{
    public interface IPowerShell
    {
        Task ExecuteAsync(string script, bool runAsAdmin = false);
        Task<TResult> ExecuteAsync<TResult>(string script);
    }

    public class PowerShell : IPowerShell
    {
        private readonly IProcessRunner processRunner;
        private readonly IConsoleLogger consoleLogger;

        public PowerShell(IProcessRunner processRunner, IConsoleLogger consoleLogger)
        {
            this.processRunner = processRunner;
            this.consoleLogger = consoleLogger;
        }

        public async Task ExecuteAsync(string script, bool runAsAdmin = false)
        {
            await InternalExecuteAsync(script, runAsAdmin);
        }

        public async Task<TResult> ExecuteAsync<TResult>(string script)
        {
            var result = await InternalExecuteAsync(script, false);

            return Map<TResult>(result.LastOutput);
        }

        private async Task<ProcessResult> InternalExecuteAsync(string script, bool runAsAdmin)
        {
            var processInstructions = new ProcessInstructions
            {
                RunAsAdmin = runAsAdmin,
                Executable = "pwsh.exe",
                Arguments = $@"-Command ""{script}"""
            };

            var result = await processRunner.ExecuteAsync(processInstructions);

            if (result.ExitCode != 0)
                throw new Exception($"Script failed to complete with exit code {result.ExitCode}");

            result.Errors.ForEach(consoleLogger.Error);

            return result;
        }

        private static TResult Map<TResult>(string? output)
        {
            if (output == null)
                return default!;

            var resultType = typeof(TResult);
            object objResult = resultType switch
            {
                { } when resultType == typeof(string) => output,
                { } when resultType == typeof(bool) => bool.Parse(output),
                _ => throw new NotSupportedException(
                    $"PowerShell result type of '{typeof(TResult).FullName}' is not yet supported")
            };

            return (TResult)objResult;
        }
    }
}
