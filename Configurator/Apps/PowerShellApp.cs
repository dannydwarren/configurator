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

        string? IApp.InstallArgs => throw new NotSupportedException();

        bool IApp.PreventUpgrade => throw new NotSupportedException();

        AppConfiguration? IApp.Configuration => throw new NotSupportedException();
    }
}
