using System;
using System.IO;
using System.Threading.Tasks;

namespace Configurator.Utilities
{
    public interface IScriptToFileConverter
    {
        Task<string> ToPowerShellAsync(string script);
    }

    public class ScriptToFileConverter : IScriptToFileConverter
    {
        private readonly IFileSystem fileSystem;
        private readonly ISpecialFolders specialFolders;

        public ScriptToFileConverter(IFileSystem fileSystem, ISpecialFolders specialFolders)
        {
            this.fileSystem = fileSystem;
            this.specialFolders = specialFolders;
        }
        
        public async Task<string> ToPowerShellAsync(string script)
        {
            var localAppDataPath = specialFolders.GetLocalAppDataPath();
            var localTempDir = Path.Combine(localAppDataPath, "temp");

            fileSystem.CreateDirectory(localTempDir);
            
            var fileName = $"{DateTimeOffset.Now:yyyy-MM-dd_HH-mm-ss-fffff}.ps1";
            var filePath = Path.Combine(localTempDir, fileName);

            var myDocumentsPath = specialFolders.GetMyDocumentsPath();
            var currentUserCurrentHostProfile = Path.Combine(myDocumentsPath, "WindowsPowerShell\\Microsoft.PowerShell_profile.ps1");

            var environmentReadyScript = $@"
$env:Path = [System.Environment]::GetEnvironmentVariable(""Path"",""Machine"") + "";"" + [System.Environment]::GetEnvironmentVariable(""Path"",""User"")

if ($profile -eq $null -or $profile -eq '') {{
  $global:profile = ""{currentUserCurrentHostProfile}""
}}

{script}";
            
            await fileSystem.WriteAllTextAsync(filePath, environmentReadyScript);
            
            return filePath;
        }
    }
}
