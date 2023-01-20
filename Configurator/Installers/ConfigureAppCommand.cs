using System.Threading.Tasks;

namespace Configurator.Installers;

public interface IConfigureAppCommand
{
    Task ExecuteAsync();
}

public class ConfigureAppCommand : IConfigureAppCommand
{
    public async Task ExecuteAsync()
    {
        throw new System.NotImplementedException();
    }
}
