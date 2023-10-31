using Configurator.Apps;
using System.Collections.Generic;

namespace Configurator;

public class Manifest_V2
{
    public List<string> AppIds { get; init; } = new();
    public List<IApp> Apps { get; init; } = new();
}
