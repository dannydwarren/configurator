using System;
using System.Threading.Tasks;
using Configurator.Utilities;

namespace Configurator.PowerShell
{
    public interface IPowerShell
    {
        Task ExecuteAsync(string script);
        Task ExecuteAdminAsync(string script);
        Task<TResult> ExecuteAsync<TResult>(string script);
        Task ExecuteWindowsAsync(string script);
        Task ExecuteWindowsAdminAsync(string script);
        Task<TResult> ExecuteWindowsAsync<TResult>(string script);
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

        public async Task ExecuteAsync(string script)
        {
            var processInstructions = BuildCoreProcessInstructions(script, runAsAdmin: false);
            await ExecuteInstructionsAsync(processInstructions);
        }

        public async Task ExecuteAdminAsync(string script)
        {
            var processInstructions = BuildCoreProcessInstructions(script, runAsAdmin: true);
            await ExecuteInstructionsAsync(processInstructions);
        }

        public async Task<TResult> ExecuteAsync<TResult>(string script)
        {
            var processInstructions = BuildCoreProcessInstructions(script, false);
            var result = await ExecuteInstructionsAsync(processInstructions);

            return Map<TResult>(result.LastOutput);
        }
        
        public async Task ExecuteWindowsAsync(string script)
        {
            var processInstructions = BuildWindowsProcessInstructions(script, runAsAdmin: false);
            await ExecuteInstructionsAsync(processInstructions);
        }

        public async Task ExecuteWindowsAdminAsync(string script)
        {
            var processInstructions = BuildWindowsProcessInstructions(script, runAsAdmin: true);
            await ExecuteInstructionsAsync(processInstructions);
        }
      
        public async Task<TResult> ExecuteWindowsAsync<TResult>(string script)
        {
            var processInstructions = BuildCoreProcessInstructions(script, false);
            var result = await ExecuteInstructionsAsync(processInstructions);

            return Map<TResult>(result.LastOutput);
        }
  
        private static ProcessInstructions BuildCoreProcessInstructions(string script, bool runAsAdmin)
        {
            var processInstructions = new ProcessInstructions
            {
                RunAsAdmin = runAsAdmin,
                Executable = "pwsh.exe",
                Arguments = $@"-Command {script}"
            };
            return processInstructions;
        }

        private static ProcessInstructions BuildWindowsProcessInstructions(string script, bool runAsAdmin)
        {
            var processInstructions = new ProcessInstructions
            {
                RunAsAdmin = runAsAdmin,
                Executable = "powershell.exe",
                Arguments = $@"""{script}"""
            };
            return processInstructions;
        }
        
        private async Task<ProcessResult> ExecuteInstructionsAsync(ProcessInstructions instructions)
        {
            var result = await processRunner.ExecuteAsync(instructions);

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
