using Configurator.Apps;
using System.Collections.Generic;

namespace Configurator;

public class Manifest
{
    public List<string> AppIds { get; init; } = new();
    public List<IApp> Apps { get; init; } = new();
}
