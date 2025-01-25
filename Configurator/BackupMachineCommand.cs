using System.Threading.Tasks;

namespace Configurator;

public interface IBackupMachineCommand
{
    Task ExecuteAsync();
}

public class BackupMachineCommand : IBackupMachineCommand
{
    public Task ExecuteAsync()
    {
        throw new System.NotImplementedException();
    }
}
