﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;
using Configurator.Windows;
using Moq;
using Shouldly;
using Xunit;

namespace Configurator.UnitTests
{
    public class CliTests : UnitTestBase<Cli>
    {
        [Fact]
        public async Task When_launching_and_the_configurator_throws()
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .ReturnsAsync(serviceProviderMock.Object);

            machineConfiguratorMock.Setup(x => x.ExecuteAsync()).Throws<Exception>();
            
            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync());

            It($"returns the {nameof(ErrorCode.GenericFailure)} error code",
                () => result.ShouldBe(ErrorCode.GenericFailure));
        }
        
        [Fact]
        public async Task When_launching_with_elevated_privileges()
        {
            GetMock<IPrivilegesRepository>().Setup(x => x.UserHasElevatedPrivileges()).Returns(true);
            
            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync());

            It($"returns the {nameof(ErrorCode.TooManyPrivileges)} error code",
                () => result.ShouldBe(ErrorCode.TooManyPrivileges));

            It("logs an error to the user",
                () => GetMock<IConsoleLogger>().Verify(x =>
                    x.Error($"{nameof(Configurator)} {nameof(Cli)} must be run without elevated privileges.")));
        }
        
        [Fact]
        public async Task When_launching_with_no_commandline_args()
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync());

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
        public async Task When_launching_with_manifest_path_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var commandlineArgs = new[] { alias, RandomString() };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().ManifestPath.ShouldBe(commandlineArgs[1]));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--environments")]
        [InlineData("-e")]
        public async Task When_launching_with_environments_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var commandlineArgs = new[] { alias, RandomString() };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().Environments
                    .ShouldBe(new List<string> { commandlineArgs[1] }));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_launching_with_multiple_environments_commandline_args()
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var env1 = RandomString();
            var env2 = RandomString();
            var commandlineArgs = new[] { "--environments", $"{env1}|{env2}" };

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
        public async Task When_launching_with_downloads_dir_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var commandlineArgs = new[] { alias, RandomString() };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().DownloadsDir.ShouldBe(commandlineArgs[1]));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Theory]
        [InlineData("--single-app-id")]
        [InlineData("-app")]
        public async Task When_launching_with_target_app_commandline_args(string alias)
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var commandlineArgs = new[] { alias, RandomString() };

            IArguments? capturedArguments = null;
            GetMock<IDependencyBootstrapper>().Setup(x => x.InitializeAsync(IsAny<IArguments>()))
                .Callback<IArguments>(arguments => capturedArguments = arguments)
                .ReturnsAsync(serviceProviderMock.Object);

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("populates arguments correctly",
                () => capturedArguments.ShouldNotBeNull().SingleAppId.ShouldBe(commandlineArgs[1]));

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

            It("executes the set setting command", () => setSettingCommandMock.Verify(x => x.ExecuteAsync(settingNameArg, settingValueArg)));

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

            var commandlineArgs = new[] { "settings", "list"};

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("executes the list settings workflow", () => listSettingsWorkflowMock.Verify(x => x.ExecuteAsync()));

            It("returns a success result", () => result.ShouldBe(0));
        }

        [Fact]
        public async Task When_backing_up()
        {
            var machineConfiguratorMock = GetMock<IMachineConfigurator>();

            var serviceProviderMock = GetMock<IServiceProvider>();
            serviceProviderMock.Setup(x => x.GetService(typeof(IMachineConfigurator)))
                .Returns(machineConfiguratorMock.Object);

            var commandlineArgs = new[] { "backup" };

            var result = await BecauseAsync(() => ClassUnderTest.LaunchAsync(commandlineArgs));

            It("activates the backup command",
                () => GetMock<IConsoleLogger>().Verify(x => x.Debug("Support for backing up apps is in progress...")));

            It("returns a success result", () => result.ShouldBe(0));
        }
    }
}
