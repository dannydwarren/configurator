using System;

namespace Configurator.Apps
{
    public class PowerShellApp : IApp
    {
        public string AppId { get; set; }

        public Shell Shell => Shell.PowerShell;

        public string InstallScript { get; set; }

        public string? VerificationScript { get; set; }

        public string? UpgradeScript { get; set; }

        string? IApp.InstallArgs => null;

        bool IApp.PreventUpgrade => false;

        AppConfiguration? IApp.Configuration => null;
    }
}
