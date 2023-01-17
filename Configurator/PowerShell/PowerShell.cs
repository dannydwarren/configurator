using System;
using System.IO;
using System.Threading.Tasks;
using Configurator.Utilities;
using Configurator.Windows;

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
        private readonly IScriptToFileConverter scriptToFileConverter;
        private readonly IRegistryRepository registryRepository;
        private readonly ISpecialFolders specialFolders;
        private readonly IConsoleLogger consoleLogger;

        public PowerShell(IProcessRunner processRunner,
            IScriptToFileConverter scriptToFileConverter,
            IRegistryRepository registryRepository,
            ISpecialFolders specialFolders,
            IConsoleLogger consoleLogger)
        {
            this.processRunner = processRunner;
            this.scriptToFileConverter = scriptToFileConverter;
            this.registryRepository = registryRepository;
            this.specialFolders = specialFolders;
            this.consoleLogger = consoleLogger;
        }
//https://devblogs.microsoft.com/powershell/depending-on-the-right-powershell-nuget-package-in-your-net-project/
        public async Task ExecuteAsync(string script)
        {
            var processInstructions = await BuildCoreProcessInstructionsAsync(script, runAsAdmin: false);
            await ExecuteInstructionsAsync(processInstructions);
        }

        public async Task ExecuteAdminAsync(string script)
        {
            var processInstructions = await BuildCoreProcessInstructionsAsync(script, runAsAdmin: true);
            await ExecuteInstructionsAsync(processInstructions);
        }

        public async Task<TResult> ExecuteAsync<TResult>(string script)
        {
            var processInstructions = await BuildCoreProcessInstructionsAsync(script, runAsAdmin: false);
            var result = await ExecuteInstructionsAsync(processInstructions);

            return Map<TResult>(result.LastOutput);
        }
        
        public async Task ExecuteWindowsAsync(string script)
        {
            var processInstructions = await BuildWindowsProcessInstructionsAsync(script, runAsAdmin: false);
            await ExecuteInstructionsAsync(processInstructions);
        }

        public async Task ExecuteWindowsAdminAsync(string script)
        {
            var processInstructions = await BuildWindowsProcessInstructionsAsync(script, runAsAdmin: true);
            await ExecuteInstructionsAsync(processInstructions);
        }
      
        public async Task<TResult> ExecuteWindowsAsync<TResult>(string script)
        {
            var processInstructions = await BuildWindowsProcessInstructionsAsync(script, runAsAdmin: false);
            var result = await ExecuteInstructionsAsync(processInstructions);

            return Map<TResult>(result.LastOutput);
        }
  
        private async Task<ProcessInstructions> BuildCoreProcessInstructionsAsync(string script, bool runAsAdmin)
        {
            var environmentReadyScript = BuildEnvironmentReadyScript("PowerShell", script);
            
            var scriptFile = await scriptToFileConverter.ToPowerShellAsync(environmentReadyScript);

            var powerShellCoreInstallLocation = registryRepository.GetValue(
                @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShellCore\InstalledVersions\31ab5147-9a97-4452-8443-d9709f0516e1",
                "InstallLocation");
            
            var processInstructions = new ProcessInstructions
            {
                RunAsAdmin = runAsAdmin,
                Executable = $"{powerShellCoreInstallLocation}pwsh.exe",
                Arguments = $@"-File {scriptFile}"
            };
            return processInstructions;
        }

        private async Task<ProcessInstructions> BuildWindowsProcessInstructionsAsync(string script, bool runAsAdmin)
        {
            var environmentReadyScript = BuildEnvironmentReadyScript("WindowsPowerShell", script);

            var scriptFile = await scriptToFileConverter.ToPowerShellAsync(environmentReadyScript);

            var processInstructions = new ProcessInstructions
            {
                RunAsAdmin = runAsAdmin,
                Executable = "powershell.exe",
                Arguments = $@"-File {scriptFile}"
            };
            return processInstructions;
        }

        private string BuildEnvironmentReadyScript(string myDocumentsFolderName, string script)
        {
            var myDocumentsPath = specialFolders.GetMyDocumentsPath();
            var currentUserCurrentHostProfile =
                Path.Combine(myDocumentsPath, $"{myDocumentsFolderName}\\Microsoft.PowerShell_profile.ps1");

            var environmentReadyScript = $@"
$env:Path = [System.Environment]::GetEnvironmentVariable(""Path"",""Machine"") + "";"" + [System.Environment]::GetEnvironmentVariable(""Path"",""User"")

if ($profile -eq $null -or $profile -eq '') {{
  $global:profile = ""{currentUserCurrentHostProfile}""
}}

{script}";
            return environmentReadyScript;
        }

        private async Task<ProcessResult> ExecuteInstructionsAsync(ProcessInstructions instructions)
        {
            var result = await processRunner.ExecuteAsync(instructions);

            result.Errors.ForEach(consoleLogger.Error);

            if (result.ExitCode != 0)
                throw new Exception($"Script failed to complete with exit code {result.ExitCode}");

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
