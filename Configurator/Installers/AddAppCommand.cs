using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configurator.Installers;

public interface IAddAppCommand
{
    Task ExecuteAsync(string appId, AppType appType, List<string> environments);
}

public class AddAppCommand : IAddAppCommand
{
    private readonly IManifestRepository manifestRepository;

    public AddAppCommand(IManifestRepository manifestRepository)
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
