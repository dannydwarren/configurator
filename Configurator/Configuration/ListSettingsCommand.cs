using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Utilities;

namespace Configurator.Configuration
{
    public interface IListSettingsCommand
    {
        Task ExecuteAsync();
    }

    public class ListSettingsCommand : IListSettingsCommand
    {
        private readonly ISettingsRepository settingsRepository;
        private readonly IConsoleLogger consoleLogger;

        public ListSettingsCommand(ISettingsRepository settingsRepository, IConsoleLogger consoleLogger)
        {
            this.settingsRepository = settingsRepository;
            this.consoleLogger = consoleLogger;
        }

        public async Task ExecuteAsync()
        {
            var settings = await settingsRepository.LoadSettingsAsync();

            var settingRows = Map(settings);

            consoleLogger.Table(settingRows);
        }

        private static List<SettingRow> Map(Settings settings)
        {
            var settingRows = new List<SettingRow>
            {
                new SettingRow
                {
                    Name = $"{nameof(settings.Manifest).ToLower()}.{nameof(settings.Manifest.Repo).ToLower()}",
                    Value = settings.Manifest.Repo?.ToString() ?? "",
                    Type = settings.Manifest.Repo?.GetType().ToString() ?? ""
                }
            };
            return settingRows;
        }

        public class SettingRow
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
        }
    }
}
