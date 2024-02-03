using System;

namespace Configurator.Configuration
{
    public class Settings
    {
        public Uri DownloadsDirectory { get; set; } = new Uri("C:/tmp/configurator-downloads", UriKind.Absolute);
        public ManifestSettings Manifest { get; set; } = new ManifestSettings();
        public GitSettings Git { get; set; } = new GitSettings();
    }

    public class ManifestSettings
    {
        public Uri? Repo { get; set; }
        public string FileName { get; set; } = "manifest.json";
        public string? Directory { get; set; }
    }

    public class GitSettings
    {
        public Uri CloneDirectory { get; set; } = new Uri("C:/src", UriKind.Absolute);
    }
}
