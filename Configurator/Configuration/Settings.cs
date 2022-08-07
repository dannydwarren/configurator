using System;

namespace Configurator.Configuration
{
    public class Settings
    {
        public ManifestSettings Manifest { get; set; } = new ManifestSettings();
    }

    public class ManifestSettings
    {
        public Uri? Repo { get; set; } = null;
    }
}
