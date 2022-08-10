using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Configurator.Configuration
{
    public interface ISetSettingCommand
    {
        Task ExecuteAsync(string settingName, string settingValue);
    }

    public class SetSettingCommand : ISetSettingCommand
    {
        private readonly ISettingsRepository settingsRepository;

        public SetSettingCommand(ISettingsRepository settingsRepository)
        {
            this.settingsRepository = settingsRepository;
        }

        public async Task ExecuteAsync(string settingName, string settingValue)
        {
            var settings = await settingsRepository.LoadSettingsAsync();

            if(!FindAndSet(settingName, settingValue, settings))
            {
                throw new ArgumentException($"{settingName} is not a recognized setting name.", "setting-name");
            }

            await settingsRepository.SaveAsync(settings);
        }

        private static bool FindAndSet(string settingPath, string settingValue, object parentNode, string parentPrefix = "")
        {
            var parentType = parentNode.GetType();
            var parentProperties = parentType.GetProperties();
            var leafProperties = parentProperties.Where(IsLeafProperty).ToList();
            var nodeProperties = parentProperties.Except(leafProperties).ToList();

            var setting = leafProperties.SingleOrDefault(x => BuildPropertyPath(parentPrefix, x) == settingPath);
            if (setting != null)
            {
                SetValue(settingValue, parentNode, setting);

                return true;
            }

            foreach (var nodeProperty in nodeProperties)
            {
                object? nodeValue = nodeProperty.GetValue(parentNode);
                if (nodeValue == null)
                    throw new Exception("We didn't expect this to happen");

                var newParentPrefix = BuildPropertyPath(parentPrefix, nodeProperty);

                if (FindAndSet(settingPath, settingValue, nodeValue, newParentPrefix))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetValue(string settingValue, object parentNode, PropertyInfo setting)
        {
            object typedValue = setting.PropertyType.Name switch
            {
                "Uri" => new Uri(settingValue),
                _ => settingValue
            };

            setting.SetValue(parentNode, typedValue);
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
    }
}
