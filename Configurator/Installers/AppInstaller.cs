using System.Linq;
using System.Threading.Tasks;
using Configurator.Apps;
using Configurator.PowerShell;
using Configurator.Utilities;

namespace Configurator.Installers
{
    public interface IAppInstaller
    {
        Task InstallOrUpgradeAsync(IApp app);
    }
    
    public interface IAppInstallerForceWindowsPowerShell
    {
        Task InstallOrUpgradeAsync(IApp app);
    }
    
    public class AppInstaller : IAppInstaller, IAppInstallerForceWindowsPowerShell
    {
        private readonly IPowerShell powerShell;
        private readonly IConsoleLogger consoleLogger;
        private readonly IDesktopRepository desktopRepository;

        public AppInstaller(IPowerShell powerShell, IConsoleLogger consoleLogger, IDesktopRepository desktopRepository)
        {
            this.powerShell = powerShell;
            this.consoleLogger = consoleLogger;
            this.desktopRepository = desktopRepository;
        }

        public async Task InstallOrUpgradeAsync(IApp app)
        {
            await RunAsync(app, forceWindowsPowerShell: false);
        }
        
        async Task IAppInstallerForceWindowsPowerShell.InstallOrUpgradeAsync(IApp app)
        {
            await RunAsync(app, forceWindowsPowerShell: true);
        }

        private async Task RunAsync(IApp app, bool forceWindowsPowerShell)
        {
            consoleLogger.Info($"Installing '{app.AppId}'");
            var preInstallDesktopSystemEntries = desktopRepository.LoadSystemEntries();

            var preInstallVerificationResult = await VerifyAppAsync(app, forceWindowsPowerShell);

            var actionScript = GetActionScript(app, preInstallVerificationResult);

            if (!string.IsNullOrWhiteSpace(actionScript))
            {
                if (forceWindowsPowerShell)
                {
                    await powerShell.ExecuteWindowsAsync(actionScript);
                }
                else
                {
                    await powerShell.ExecuteAsync(actionScript);
                }
                var postInstallVerificationResult = await VerifyAppAsync(app, forceWindowsPowerShell);
                if (!postInstallVerificationResult)
                {
                    consoleLogger.Debug($"Failed to install '{app.AppId}'");
                }
            }

            var postInstallDesktopSystemEntries = desktopRepository.LoadSystemEntries();
            var desktopSystemEntriesToDelete = postInstallDesktopSystemEntries.Except(preInstallDesktopSystemEntries).ToList();
            if (desktopSystemEntriesToDelete.Any())
            {
                desktopRepository.DeletePaths(desktopSystemEntriesToDelete);
            }

            consoleLogger.Result($"Installed '{app.AppId}'");
        }

        private static string GetActionScript(IApp app, bool preInstallVerificationResult)
        {
            var actionScript = "";

            if (!preInstallVerificationResult)
            {
                actionScript = app.InstallScript;
            }
            else if (app.UpgradeScript != null && !app.PreventUpgrade)
            {
                actionScript = app.UpgradeScript;
            }

            return actionScript;
        }

        private async Task<bool> VerifyAppAsync(IApp app, bool forceWindowsPowerShell)
        {
            if (app.VerificationScript == null)
                return false;

            var verificationResult = forceWindowsPowerShell 
                ? await powerShell.ExecuteWindowsAsync<bool>(app.VerificationScript)
                : await powerShell.ExecuteAsync<bool>(app.VerificationScript);
            return verificationResult;
        }
    }
}
