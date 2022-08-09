using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var settingRows = new List<SettingRow>();

            settingRows.AddRange(MapReflection(settings));

            return settingRows;
        }

        private static List<SettingRow> MapReflection(object setting, string parentPrefix = "")
        {
            var settingsType = setting.GetType();
            var settingsProperties = settingsType.GetProperties();
            var settingsPropertiesPrimitive = settingsProperties.Where(x => x.PropertyType.IsPrimitive || x.PropertyType == typeof(Uri)).ToList();
            var settingsPropertiesComplex = settingsProperties.Where(x => !x.PropertyType.IsPrimitive && x.PropertyType != typeof(Uri)).ToList();

            var settingRows = settingsPropertiesPrimitive.Select(x => new SettingRow
            {
                Name = BuildPropertyPath(parentPrefix, x),
                Value = x.GetValue(setting)?.ToString() ?? "",
                Type = x.PropertyType.ToString()
            }).ToList();
            
            settingRows.AddRange(settingsPropertiesComplex.SelectMany(x =>
            {
                object? value = x.GetValue(setting);
                if (value == null)
                    throw new Exception("We didn't expect this to happen");

                var newParentPrefix = BuildPropertyPath(parentPrefix, x);
                
                return MapReflection(value, newParentPrefix);
            }).ToList());

            return settingRows;
        }

        private static string BuildPropertyPath(string parentPrefix, PropertyInfo x)
        {
            return parentPrefix == ""
                ? x.Name.ToLower()
                : $"{parentPrefix}.{x.Name.ToLower()}";
        }

        public class SettingRow
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
        }
    }
}
