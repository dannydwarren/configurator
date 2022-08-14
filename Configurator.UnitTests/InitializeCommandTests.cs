using System;
using System.IO;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.PowerShell;
using Configurator.Utilities;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests
{
    public class InitializeCommandTests : UnitTestBase<InitializeCommand>
    {
        [Fact]
        public async Task When_initializing_for_the_first_time()
        {
            var repoName = RandomString();

            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Repo = new Uri($"https://github.com/{RandomString()}/{repoName}.git"),
                },
                Git = new GitSettings
                {
                    CloneDirectory = RandomUri()
                }
            };

            var expectedManifestDirectory =
                Path.Combine(settings.Git.CloneDirectory.AbsolutePath, repoName);

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

            Settings? capturedSavedSettings = null;
            GetMock<ISettingsRepository>().Setup(x => x.SaveAsync(IsAny<Settings>()))
                .Callback<Settings>(savedSettings => capturedSavedSettings = savedSettings);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync());

            It("initializes the system", () => GetMock<ISystemInitializer>().Verify(x => x.InitializeAsync()));

            It("sets manifest directory", () => capturedSavedSettings.ShouldNotBeNull()
                .Manifest.Directory.ShouldBe(expectedManifestDirectory));

            It("clones the manifest repo", () =>
            {
                GetMock<IPowerShell>().Verify(x => x.ExecuteAsync($@"
Push-Location {settings.Git.CloneDirectory.AbsolutePath}
git clone {settings.Manifest.Repo.AbsoluteUri}
Pop-Location"));
            });
        }

        [Fact]
        public async Task When_initializing_with_a_new_manifest_repo()
        {
            var repoName = RandomString();
            
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Repo = new Uri($"https://github.com/{RandomString()}/{repoName}.git"),
                    FileName = RandomString()
                },
                Git = new GitSettings
                {
                    CloneDirectory = RandomUri()
                }
            };

            var manifestDirectory =
                Path.Combine(settings.Git.CloneDirectory.AbsolutePath, repoName);
            var fullyQualifiedManifestFilePath =
                Path.Combine(manifestDirectory, settings.Manifest.FileName);

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

            GetMock<IFileSystem>()
                .Setup(x => x.Exists(fullyQualifiedManifestFilePath))
                .Returns(false);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync());

            It("creates, commits, and pushes the manifest file", () =>
            {
                GetMock<IFileSystem>().Verify(x => x.WriteAllTextAsync(fullyQualifiedManifestFilePath, "{ }"));
                GetMock<IPowerShell>().Verify(x => x.ExecuteAsync($@"
Push-Location {manifestDirectory}
git add .
git commit -m '[Configurator] Create manifest file'
git push
Pop-Location"));
            });
        }
        
        [Fact]
        public async Task When_initializing_and_the_system_has_already_been_initialized()
        {
            var repoName = RandomString();
            var cloneDirectory = RandomUri();
            
            var manifestDirectory =
                Path.Combine(cloneDirectory.AbsolutePath, repoName);

            var repo = new Uri($"https://github.com/{RandomString()}/{repoName}.git");
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Repo = repo,
                    Directory = manifestDirectory,
                    FileName = RandomString()
                },
                Git = new GitSettings
                {
                    CloneDirectory = cloneDirectory
                }
            };

            var fullyQualifiedManifestFilePath =
                Path.Combine(manifestDirectory, settings.Manifest.FileName);

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

            GetMock<IFileSystem>()
                .Setup(x => x.Exists(manifestDirectory))
                .Returns(true);

            GetMock<IFileSystem>()
                .Setup(x => x.Exists(fullyQualifiedManifestFilePath))
                .Returns(true);

            await BecauseAsync(() => ClassUnderTest.ExecuteAsync());

            It("does not change settings or modify the file system",
                () =>
                {
                    GetMock<ISettingsRepository>().VerifyNever(x => x.SaveAsync(IsAny<Settings>()));
                    GetMock<IFileSystem>().VerifyNever(x => x.WriteAllTextAsync(IsAny<string>(), IsAny<string>()));
                    GetMock<IPowerShell>().VerifyNever(x => x.ExecuteAsync(IsAny<string>()));
                });
        }

        [Fact]
        public async Task When_initializing_without_setting_manifest_repo()
        {
            var settings = new Settings
            {
                Manifest = new ManifestSettings
                {
                    Repo = null
                }
            };

            GetMock<ISettingsRepository>().Setup(x => x.LoadSettingsAsync()).ReturnsAsync(settings);

            var exception = await BecauseThrowsAsync<Exception>(() => ClassUnderTest.ExecuteAsync());

            It("declares manifest.repo required", () =>
                exception.ShouldNotBeNull()
                    .Message.ShouldBe("The 'manifest.repo' setting must be set before invoking initialize."));
        }
    }
}
