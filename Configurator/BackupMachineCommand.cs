using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Installers;

namespace Configurator;

public interface IBackupMachineCommand
{
    Task ExecuteAsync();
}

public class BackupMachineCommand : IBackupMachineCommand
{
    private readonly IManifestRepository manifestRepository;
    private readonly IAppConfigurator appConfigurator;

    public BackupMachineCommand(IManifestRepository manifestRepository, IAppConfigurator appConfigurator)
    {
        this.manifestRepository = manifestRepository;
        this.appConfigurator = appConfigurator;
    }
    
    public async Task ExecuteAsync()
    {
        var manifest = await manifestRepository.LoadAsync(new List<string>());

        foreach (var app in manifest.Apps)  
        {
            await appConfigurator.Backup(app);
        }
    }
}
