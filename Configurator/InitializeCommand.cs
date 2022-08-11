using System;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.PowerShell;

namespace Configurator
{
    public interface IInitializeCommand
    {
        Task ExecuteAsync();
    }

    public class InitializeCommand : IInitializeCommand
    {
        private readonly ISystemInitializer systemInitializer;
        private readonly ISettingsRepository settingsRepository;
        private readonly IPowerShell powerShell;

        public InitializeCommand(ISystemInitializer systemInitializer,
            ISettingsRepository settingsRepository,
            IPowerShell powerShell)
        {
            this.systemInitializer = systemInitializer;
            this.settingsRepository = settingsRepository;
            this.powerShell = powerShell;
        }
        
        public async Task ExecuteAsync()
        {
            await systemInitializer.InitializeAsync();

            var settings = await settingsRepository.LoadSettingsAsync();

            if (settings.Manifest.Repo == null)
            {
                throw new Exception("The 'manifest.repo' setting must be set before invoking initialize.");
            }
            
            await powerShell.ExecuteAsync($@"
Push-Location {settings.Git.CloneDirectory.AbsolutePath}
git clone {settings.Manifest.Repo.AbsoluteUri}
Pop-Location");
        }
    }
}
