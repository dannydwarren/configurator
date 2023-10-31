using Configurator.Utilities;
using System.Threading.Tasks;

namespace Configurator.Configuration
{
    public interface IConfigureMachineCommand
    {
        Task ExecuteAsync();
    }

    public class ConfigureMachineCommand : IConfigureMachineCommand
    {
        public IConsoleLogger Logger { get; }

        public ConfigureMachineCommand(IConsoleLogger logger)
        {
            Logger = logger;
        }

        public async Task ExecuteAsync()
        {
            Logger.Debug(nameof(ConfigureMachineCommand));
        }
    }
}
