using Configurator.Apps;
using System.Collections.Generic;

namespace Configurator;

public class Manifest_V2
{
    public List<string> Apps { get; init; } = new();
    public List<IApp> InstallableApps { get; init; } = new();
}
