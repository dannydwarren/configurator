using System.Linq;

namespace Configurator.Apps;

public class GitRepoApp : IApp
{
    public string AppId { get; set; }
    public string? InstallArgs => null;
    public bool PreventUpgrade => false;

    private string RepoName => AppId.Replace(".git", "").Split('\\', '/').Last();
    public string InstallScript => $@"pushd c:\src;git clone {AppId};popd";
    public string UpgradeScript => $@"pushd c:\src{RepoName};git pull;popd";
    public string VerificationScript => $@"Test-Path c:\src\{RepoName}";

    public AppConfiguration? Configuration => null;
}
