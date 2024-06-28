using System.Linq;

namespace Configurator.Apps;

public class GitRepoApp : IApp
{
    public string AppId { get; set; }
    public string? InstallArgs { get; set; }
    public bool PreventUpgrade { get; set; }

    private string cloneRootDirectory = "";
    public string CloneRootDirectory
    {
        get => cloneRootDirectory;
        set
        {
            cloneRootDirectory = value;
            var endsWithTrailingSlash = value.EndsWith('\\') || value.EndsWith('/');
            if (!endsWithTrailingSlash)
            {
                cloneRootDirectory += '\\';
            }
        }
    }

    private string RepoName => InstallArgs!.Replace(".git", "").Split('\\', '/').Last();
    public string InstallScript => $@"mkdir {CloneRootDirectory} -Force;pushd {CloneRootDirectory};git clone {InstallArgs};popd";
    public string UpgradeScript => $@"pushd {CloneRootDirectory}{RepoName};git pull;popd";
    public string VerificationScript => $@"Test-Path {CloneRootDirectory}{RepoName}";

    public AppConfiguration? Configuration => null;
}
