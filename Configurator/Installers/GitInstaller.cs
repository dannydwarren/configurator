using System.Threading.Tasks;
using Configurator.Apps;

namespace Configurator.Installers
{
    public interface IGitInstaller
    {
        Task InstallAsync();
    }

    public class GitInstaller : IGitInstaller
    {
        private readonly IAppInstaller appInstaller;
        
        public static readonly IApp GitWingetApp = new WingetApp
        {
            AppId = "Git.Git"
        };

        public GitInstaller(IAppInstaller appInstaller)
        {
            this.appInstaller = appInstaller;
        }

        public async Task InstallAsync()
        {
            await appInstaller.InstallOrUpgradeAsync(GitWingetApp);
        }
    }
}
