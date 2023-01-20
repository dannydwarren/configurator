using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configurator.Installers;

public interface IConfigureAppCommand
{
    Task ExecuteAsync(string appId, AppType appType, List<string> environments);
}

public class ConfigureAppCommand : IConfigureAppCommand
{
    private readonly IManifestRepository_V2 manifestRepository;

    public ConfigureAppCommand(IManifestRepository_V2 manifestRepository)
    {
        this.manifestRepository = manifestRepository;
    }
    
    public async Task ExecuteAsync(string appId, AppType appType, List<string> environments)
    {
        var installable = new Installable
        {
            AppId = appId,
            AppType = appType,
            Environments = string.Join("|", environments)
        };
        
        await manifestRepository.SaveInstallableAsync(installable);
    }
}
