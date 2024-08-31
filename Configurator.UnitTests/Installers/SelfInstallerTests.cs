using Castle.Components.DictionaryAdapter;
using Configurator.Apps;
using Configurator.Downloaders;
using Configurator.Installers;
using Configurator.Utilities;
using Shouldly;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Configurator.UnitTests.Installers
{
    public class SelfInstallerTests : UnitTestBase<SelfInstaller>
    {
        private static readonly string ExpectedConfiguratorInstallationDirectory = "c:\\Configurator";

        [Fact]
        public async Task WhenInstalling()
        {
            IDownloadApp capturedApp = null!;
            GetMock<IDownloadAppInstaller>().Setup(x => x.InstallOrUpgradeAsync(IsAny<IDownloadApp>()))
                .Callback<IDownloadApp>(app =>
                {
                    capturedApp = app;
                    SimulateDownloaderSettingDownloadedFilePath(capturedApp);
                });

            var fileSystemMock = GetMock<IFileSystem>();
            fileSystemMock.Setup(x => x.Exists(ExpectedConfiguratorInstallationDirectory)).Returns(false);

            await BecauseAsync(() => ClassUnderTest.InstallAsync());

            It("downloads for installation", () =>
            {
                capturedApp.ShouldSatisfyAllConditions(x =>
                {
                    x.AppId.ShouldBe("Configurator");

                    x.Downloader.ShouldBe(nameof(GitHubAssetDownloader));
                    x.DownloaderArgs.GetProperty(nameof(GitHubAssetDownloaderArgs.User)).GetString().ShouldBe("dannydwarren");
                    x.DownloaderArgs.GetProperty(nameof(GitHubAssetDownloaderArgs.Repo)).GetString().ShouldBe("configurator");
                    x.DownloaderArgs.GetProperty(nameof(GitHubAssetDownloaderArgs.Extension)).GetString().ShouldBe(".exe");
                });
            });

            It("moves to installation location", () =>
            {
                fileSystemMock.Verify(x => x.CreateDirectory(ExpectedConfiguratorInstallationDirectory));
                fileSystemMock.Verify(x => x.MoveFile(capturedApp.DownloadedFilePath, Path.Combine(ExpectedConfiguratorInstallationDirectory, "Configurator.exe")));
            });
        }

        [Fact]
        public async Task WhenInstalling_AndItIsAlreadyInstalled()
        {
            var fileSystemMock = GetMock<IFileSystem>();
            fileSystemMock.Setup(x => x.Exists(ExpectedConfiguratorInstallationDirectory)).Returns(true);

            await BecauseAsync(() => ClassUnderTest.InstallAsync());

            It("does nothing", () =>
            {
                GetMock<IDownloadAppInstaller>().VerifyNever(x => x.InstallOrUpgradeAsync(IsAny<IDownloadApp>()));
                fileSystemMock.VerifyNever(x => x.CreateDirectory(IsAny<string>()));
            });
        }

        private void SimulateDownloaderSettingDownloadedFilePath(IDownloadApp app)
        {
            app.DownloadedFilePath = RandomString();
        }
    }
}
