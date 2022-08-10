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

        private static List<SettingRow> MapReflection(object parentNode, string parentPrefix = "")
        {
            var parentType = parentNode.GetType();
            var parentProperties = parentType.GetProperties();
            var leafProperties = parentProperties.Where(IsLeafProperty).ToList();
            var nodeProperties = parentProperties.Except(leafProperties).ToList();

            var settingRows = leafProperties.Select(x => new SettingRow
            {
                Name = BuildPropertyPath(parentPrefix, x),
                Value = x.GetValue(parentNode)?.ToString() ?? "",
                Type = x.PropertyType.Name
            }).ToList();
            
            settingRows.AddRange(nodeProperties.SelectMany(x =>
            {
                object? value = x.GetValue(parentNode);
                if (value == null)
                    throw new Exception("We didn't expect this to happen");

                var newParentPrefix = BuildPropertyPath(parentPrefix, x);
                
                return MapReflection(value, newParentPrefix);
            }).ToList());

            return settingRows;
        }

        private static bool IsLeafProperty(PropertyInfo x)
        {
            return x.PropertyType.IsPrimitive
                   || x.PropertyType == typeof(Uri)
                   || x.PropertyType == typeof(string);
        }

        private static string BuildPropertyPath(string parentPrefix, PropertyInfo property)
        {
            return parentPrefix == ""
                ? property.Name.ToLower()
                : $"{parentPrefix}.{property.Name.ToLower()}";
        }

        public class SettingRow
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public string Type { get; set; }
        }
    }
}
