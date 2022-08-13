using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Configurator.PowerShell
{
    public interface IPowerShell
    {
        Task<PowerShellResult> ExecuteAsync(string script);
    }

    public class PowerShell : IPowerShell
    {
        public async Task<PowerShellResult> ExecuteAsync(string script)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Verb = "runas",
                    FileName = @"pwsh.exe",
                    Arguments = @$"-Command ""{script}"""
                },
                EnableRaisingEvents = true
            };
            
            var result = new PowerShellResult();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Token.Register(() =>
            {
                result.ExitCode = process.ExitCode;
                process.Dispose();
            });

            process.Start();

            var outputLoop = Task.Run(() =>
            {
                while (!process.StandardOutput.EndOfStream && !process.HasExited)
                {
                    result.Output = process.StandardOutput.ReadLine();
                }
            }, cancellationTokenSource.Token);

            var exitLoop = Task.Run(() =>
            {
                process.WaitForExit();
            }, cancellationTokenSource.Token);

            await exitLoop;
            
            cancellationTokenSource.Cancel();

            await outputLoop;

            return result;
        }
    }

    public class PowerShellResult
    {
        public int ExitCode { get; set; } = -1;
        public string? Output { get; set; }
    }
}
