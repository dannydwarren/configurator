using System;

namespace Configurator.Apps
{
    public class PowerShellApp : IApp
    {
        public string AppId { get; set; }

        public string? InstallArgs => throw new NotImplementedException();

        public bool PreventUpgrade => throw new NotImplementedException();

        public string InstallScript => throw new NotImplementedException();

        public string? VerificationScript => throw new NotImplementedException();

        public string? UpgradeScript => throw new NotImplementedException();

        public AppConfiguration? Configuration => throw new NotImplementedException();
    }
}
