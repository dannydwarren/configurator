using System;
using System.Threading.Tasks;

namespace Configurator.Configuration
{
    public interface IUpdateSettingsCommand
    {
        Task ExecuteAsync(string settingName, string settingValue);
    }

    public class UpdateSettingsCommand : IUpdateSettingsCommand
    {
        private readonly ISettingsRepository settingsRepository;

        public UpdateSettingsCommand(ISettingsRepository settingsRepository)
        {
            this.settingsRepository = settingsRepository;
        }

        public async Task ExecuteAsync(string settingName, string settingValue)
        {
            var settings = await settingsRepository.LoadSettingsAsync();

            if (settingName == "manifest.repo")
            {
                settings.Manifest.Repo = new Uri(settingValue);
            }

            await settingsRepository.SaveAsync(settings);
        }
    }
}
