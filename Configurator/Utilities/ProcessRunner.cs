﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Configurator.Utilities
{
    public interface IProcessRunner
    {
        Task<ProcessResult> ExecuteAsync(ProcessInstructions instructions);
    }

    public class ProcessRunner : IProcessRunner
    {
        private readonly IConsoleLogger consoleLogger;

        public ProcessRunner(IConsoleLogger consoleLogger)
        {
            this.consoleLogger = consoleLogger;
        }

        public async Task<ProcessResult> ExecuteAsync(ProcessInstructions instructions)
        {
            var process = BuildProcess(instructions);
            var result = new ProcessResult();
            var cancellationTokenSource = BuildCancellationTokenSource(result, process);

            process.Start();

            var outputLoop = RunOutputLoop(process, result, cancellationTokenSource);
            var errorLoop = RunErrorLoop(process, result, cancellationTokenSource);
            var exitLoop = RunExitLoop(process, cancellationTokenSource);

            await exitLoop;

            cancellationTokenSource.Cancel();

            await Task.WhenAll(outputLoop, errorLoop);
            return result;
        }

        private static Process BuildProcess(ProcessInstructions instructions)
        {
            return new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = instructions.RunAsAdmin,
                    RedirectStandardOutput = !instructions.RunAsAdmin,
                    RedirectStandardError = !instructions.RunAsAdmin,
                    Verb = instructions.RunAsAdmin ? "runas" : "open",
                    FileName = instructions.Executable,
                    Arguments = instructions.Arguments
                }
            };
        }

        private static CancellationTokenSource BuildCancellationTokenSource(ProcessResult result, Process process)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Token.Register(() =>
            {
                result.ExitCode = process.ExitCode;
                process.Dispose();
            });
            return cancellationTokenSource;
        }

        private static Task RunExitLoop(Process process, CancellationTokenSource cancellationTokenSource)
        {
            return Task.Run(process.WaitForExit, cancellationTokenSource.Token);
        }

        private Task RunOutputLoop(Process process, ProcessResult result,
            CancellationTokenSource cancellationTokenSource)
        {
            return Task.Run(() =>
            {
                if (!process.StartInfo.RedirectStandardOutput)
                    return;

                while (!process.StandardOutput.EndOfStream && !process.HasExited)
                {
                    var output = process.StandardOutput.ReadLine();

                    result.LastOutput = output;

                    if (output != null)
                    {
                        result.AllOutput.Add(output);
                        consoleLogger.Debug(output);                        
                    }
                }
            }, cancellationTokenSource.Token);
        }

        private Task RunErrorLoop(Process process, ProcessResult result,
            CancellationTokenSource cancellationTokenSource)
        {
            return Task.Run(() =>
            {
                if (!process.StartInfo.RedirectStandardError)
                    return;
                
                while (!process.StandardError.EndOfStream && !process.HasExited)
                {
                    var dirtyError = process.StandardError.ReadLine();

                    if (dirtyError != null)
                    {
                        var error = CleanError(dirtyError);
                        result.Errors.Add(error);
                        consoleLogger.Debug(error);
                    }
                }
            }, cancellationTokenSource.Token);
        }

        private static string CleanError(string dirtyError)
        {
            var strangeCharacterSequenceBeginningAndMiddleOfLine = $"{(char)27}[91m";
            var strangeCharacterSequenceBeginningAndMiddleOfLine2 = $"{(char)27}[31;1m";
            var stringCharacterSequenceEndOfLine = $"{(char)27}[0m";
            var cleanError = dirtyError
                .Replace(strangeCharacterSequenceBeginningAndMiddleOfLine, "")
                .Replace(strangeCharacterSequenceBeginningAndMiddleOfLine2, "")
                .Replace(stringCharacterSequenceEndOfLine, "");

            return cleanError;
        }
    }

    public class ProcessInstructions
    {
        public bool RunAsAdmin { get; set; }
        public string Executable { get; set; }
        public string Arguments { get; set; }
    }

    public class ProcessResult
    {
        public int ExitCode { get; set; } = -1;
        public string? LastOutput { get; set; }
        public List<string> AllOutput { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }
}
