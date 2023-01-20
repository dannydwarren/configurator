using System.Threading.Tasks;

namespace Configurator;

public interface IManifestRepository_V2
{
    Task SaveInstallableAsync(Installable installable);
}

public class ManifestRepository_V2 : IManifestRepository_V2
{
    public async Task SaveInstallableAsync(Installable installable)
    {
        throw new System.NotImplementedException();
    }
    
}

public class Installable
{
    public string AppId { get; set; }
    public AppType AppType { get; set; }
    public string Environments { get; set; }
}
