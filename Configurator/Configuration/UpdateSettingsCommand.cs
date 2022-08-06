using System.Threading.Tasks;

namespace Configurator.Configuration
{
    public interface IUpdateSettingsCommand
    {
        Task ExecuteAsync();
    }

    public class UpdateSettingsCommand : IUpdateSettingsCommand
    {
        private readonly ISettingsRepository settingsRepository;

        public UpdateSettingsCommand(ISettingsRepository settingsRepository)
        {
            this.settingsRepository = settingsRepository;
        }
        
        public async Task ExecuteAsync()
        {
            var settings = await settingsRepository.LoadSettingsAsync();

            await settingsRepository.SaveAsync(settings);
        }
    }
}
