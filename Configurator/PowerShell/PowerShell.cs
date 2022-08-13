using System;
using System.Threading.Tasks;

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

        public PowerShell(IPowerShellRunner powerShellRunner)
        {
            this.powerShellRunner = powerShellRunner;
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
