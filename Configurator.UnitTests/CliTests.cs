using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Installers;
using Configurator.Utilities;
using Configurator.Windows;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests
{
    public class CliTests : UnitTestBase<Cli>
    {
        public CliTests()
        {
            GetMock<IPrivilegesRepository>().Setup(x => x.UserHasElevatedPrivileges()).Returns(true);
        }

        [Fact]
        public async Task When_launching_with_invalid_commandline_arg()
        {
            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync("invalid-arg"));

            It($"returns the {nameof(ErrorCode.GenericFailure)} error code",
                () => result.ShouldBe(ErrorCode.GenericFailure));
        }

        [Fact]
        public async Task When_launching_without_elevated_privileges()
        {
            GetMock<IPrivilegesRepository>().Setup(x => x.UserHasElevatedPrivileges()).Returns(false);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync());

            It($"returns the {nameof(ErrorCode.NotEnoughPrivileges)} error code",
                () => result.ShouldBe(ErrorCode.NotEnoughPrivileges));

            It("logs an error to the user",
                () => GetMock<IConsoleLogger>().Verify(x =>
                    x.Error($"{nameof(Configurator)} {nameof(Cli)} must be run with elevated privileges.")));
        }

        [Fact]
        public async Task When_launching_no_commandline_args()
        {
            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync());

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_launching_single_file_command_with_default_args()
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync("single-file"));

            It("populates arguments correctly", () =>
            {
                capturedArguments.ShouldNotBeNull().ShouldSatisfyAllConditions(x =>
                {
                    x.ManifestPath.ShouldBe(Arguments.Default.ManifestPath);
                    x.Environments.ShouldBe(Arguments.Default.Environments);
                    x.DownloadsDir.ShouldBe(Arguments.Default.DownloadsDir);
                    x.SingleAppId.ShouldBeNull();
                });
            });

            It("runs machine configurator",
                () => { machineConfiguratorMock.Verify(x => x.ExecuteAsync(), Times.Once); });

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--manifest-path")]
        [InlineData("-m")]
        public async Task When_launching_single_file_command_with_manifest_path_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var manifestPathArg = RandomString();
            var commandlineArgs = new[] { "single-file", alias, manifestPathArg };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().ManifestPath.ShouldBe(manifestPathArg));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--environments")]
        [InlineData("-e")]
        public async Task When_launching_single_file_command_with_environments_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var environmentArg = RandomString();
            var commandlineArgs = new[] { "single-file", alias, environmentArg };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().Environments.ShouldBe(new List<string> { environmentArg }));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_launching_single_file_command_with_multiple_environments_commandline_args()
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var env1 = RandomString();
            var env2 = RandomString();
            var commandlineArgs = new[] { "single-file", "--environments", $"{env1}|{env2}" };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().Environments.ShouldBe(new List<string> { env1, env2 }));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--downloads-dir")]
        [InlineData("-dl")]
        public async Task When_launching_single_file_command_with_downloads_dir_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var downloadDirArg = RandomString();
            var commandlineArgs = new[] { "single-file", alias, downloadDirArg };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().DownloadsDir.ShouldBe(downloadDirArg));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--single-app-id")]
        [InlineData("-app")]
        public async Task When_launching_single_file_command_with_target_app_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var singleAppIdArg = RandomString();
            var commandlineArgs = new[] { "single-file", alias, singleAppIdArg };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().SingleAppId.ShouldBe(singleAppIdArg));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_initializing()
        {
            var initializeCommandMock = GetMock<IInitializeCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IInitializeCommand)))
                .Returns(initializeCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(Arguments.Default))
                .ReturnsAsync(serviceProviderMock.Object);

            var commandlineArgs = new[] { "initialize" };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("executes the initialize command", () => initializeCommandMock.Verify(x => x.ExecuteAsync()));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_setting_settings()
        {
            var setSettingCommandMock = GetMock<ISetSettingCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(ISetSettingCommand)))
                .Returns(setSettingCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(Arguments.Default))
                .ReturnsAsync(serviceProviderMock.Object);

            var settingNameArg = RandomString();
            var settingValueArg = RandomString();
            var commandlineArgs = new[] { "settings", "set", settingNameArg, settingValueArg };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("executes the set setting command",
                () => setSettingCommandMock.Verify(x => x.ExecuteAsync(settingNameArg, settingValueArg)));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_listing_settings()
        {
            var listSettingsWorkflowMock = GetMock<IListSettingsCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IListSettingsCommand)))
                .Returns(listSettingsWorkflowMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(Arguments.Default))
                .ReturnsAsync(serviceProviderMock.Object);

            var commandlineArgs = new[] { "settings", "list" };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("executes the list settings workflow", () => listSettingsWorkflowMock.Verify(x => x.ExecuteAsync()));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_backing_up()
        {
            var commandlineArgs = new[] { "backup" };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("activates the backup command",
                () => GetMock<IConsoleLogger>().Verify(x => x.Debug("Support for backing up apps is in progress...")));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("configure-app")]
        [InlineData("configure")]
        [InlineData("add-app")]
        [InlineData("add")]
        public async Task When_configuring_an_app(string args)
        {
            var configureAppCommandMock = GetMock<IConfigureAppCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IConfigureAppCommand)))
                .Returns(configureAppCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(Arguments.Default))
                .ReturnsAsync(serviceProviderMock.Object);

            var appId = RandomString();
            var appType = AppType.Winget;
            var env1 = RandomString();
            var env2 = RandomString();
            var environments = $"{env1}|{env2}";
            var expectedEnvironments = new List<string> { env1, env2 };

            var commandlineArgs = new[]
                { args, "--app-id", appId, "--app-type", appType.ToString(), "--environments", environments };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("activates the command",
                () => GetMock<IConfigureAppCommand>().Verify(x =>
                   x.ExecuteAsync(appId, appType, IsSequenceEqual(expectedEnvironments))));

            It("returns a success result", () => result.ShouldBe(0));
        }
    }
}
