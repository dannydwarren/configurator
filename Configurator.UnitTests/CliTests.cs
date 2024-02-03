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
        public async Task When_initializing()
        {
            var initializeCommandMock = GetMock<IInitializeCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IInitializeCommand)))
                .Returns(initializeCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync())
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

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync())
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

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync())
                .ReturnsAsync(serviceProviderMock.Object);

            var commandlineArgs = new[] { "settings", "list" };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("executes the list settings workflow", () => listSettingsWorkflowMock.Verify(x => x.ExecuteAsync()));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("add-app")]
        [InlineData("add")]
        public async Task When_adding_an_app(string arg)
        {
            var addAppCommandMock = GetMock<IAddAppCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IAddAppCommand)))
                .Returns(addAppCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync())
                .ReturnsAsync(serviceProviderMock.Object);

            var appId = RandomString();
            var appType = AppType.Winget;
            var env1 = RandomString();
            var env2 = RandomString();
            var environments = $"{env1}|{env2}";
            var expectedEnvironments = new List<string> { env1, env2 };

            var commandlineArgs = new[]
                { arg, "--app-id", appId, "--app-type", appType.ToString(), "--environments", environments };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("activates the command",
                () => GetMock<IAddAppCommand>().Verify(x =>
                   x.ExecuteAsync(appId, appType, IsSequenceEqual(expectedEnvironments))));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("configure-machine")]
        [InlineData("configure")]
        public async Task When_configuring_machine_in_manifest_from_settings(string arg)
        {
            var configureMachineCommandMock = GetMock<IConfigureMachineCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IConfigureMachineCommand)))
                .Returns(configureMachineCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync())
            .ReturnsAsync(serviceProviderMock.Object);

            var commandlineArgs = new[] { arg };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("executes the configure machine command", () => configureMachineCommandMock.Verify(x => x.ExecuteAsync(new List<string>(), null!)));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--single-app-id")]
        [InlineData("-app")]
        public async Task When_configuring_single_app_in_manifest_from_settings(string alias)
        {
            var configureMachineCommandMock = GetMock<IConfigureMachineCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IConfigureMachineCommand)))
                .Returns(configureMachineCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync())
            .ReturnsAsync(serviceProviderMock.Object);

            var singleAppId = RandomString();
            var commandlineArgs = new[] { "configure", alias, singleAppId };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("executes the configure machine command for a single app", () => configureMachineCommandMock.Verify(x => x.ExecuteAsync(new List<string>(), singleAppId)));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--environments")]
        [InlineData("-e")]
        public async Task When_configuring_machine_for_specific_environments(string alias)
        {
            var configureMachineCommandMock = GetMock<IConfigureMachineCommand>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IConfigureMachineCommand)))
                .Returns(configureMachineCommandMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync())
            .ReturnsAsync(serviceProviderMock.Object);

            var environmentsOption = RandomString();
            var commandlineArgs = new[] { "configure", alias, environmentsOption };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("specifies the environments", () => configureMachineCommandMock.Verify(x => x.ExecuteAsync(new List<string> { environmentsOption }, null!)));

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
    }
}
