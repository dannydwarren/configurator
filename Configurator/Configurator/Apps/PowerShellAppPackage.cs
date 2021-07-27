﻿using System.Text.Json;

namespace Configurator.Apps
{
    public class PowerShellAppPackage : IDownloadApp
    {
        public string AppId { get; set; } = "";
        public string Environments { get; set; } = "";
        public string InstallScript => @"Import-Module appx -UseWindowsPowerShell
Add-AppPackage";
        public string VerificationScript => $@"Import-Module appx -UseWindowsPowerShell
Get-AppPackage -Name {AppId}";
        public string Downloader { get; set; } = "";
        public JsonElement DownloaderArgs { get; set; }
    }
}
