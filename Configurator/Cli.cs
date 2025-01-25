using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Installers;
using Configurator.Utilities;
using Configurator.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Configurator
{
    public class Cli
    {
        private readonly IDependencyBootstrapper dependencyBootstrapper;
        private readonly IPrivilegesRepository privilegesRepository;
        private readonly IConsoleLogger consoleLogger;

        public Cli(IDependencyBootstrapper dependencyBootstrapper,
            IPrivilegesRepository privilegesRepository,
            IConsoleLogger consoleLogger)
        {
            this.dependencyBootstrapper = dependencyBootstrapper;
            this.privilegesRepository = privilegesRepository;
            this.consoleLogger = consoleLogger;
        }

        public async Task<int> LaunchAsync(params string[] args)
        {
            if (!privilegesRepository.UserHasElevatedPrivileges())
            {
                consoleLogger.Error($"{nameof(Configurator)} {nameof(Cli)} must be run with elevated privileges.");
                return ErrorCode.NotEnoughPrivileges;
            }

            if (args.Length == 0)
            {
                args = new[] { "--help" };
            }
            
            var rootCommand = CreateRootCommand();
            return await rootCommand.InvokeAsync(args);
        }

        private RootCommand CreateRootCommand()
        {
            var rootCommand = new RootCommand("Configurator")
            {
                CreateInitializeCommand(),
                CreateSettingsCommand(),
                CreateConfigureMachineCommand(),
                CreateAddAppCommand(),
                CreateBackupCommand()
            };

            return rootCommand;
        }

        private Command CreateAddAppCommand()
        {
            var appIdOption = new Option<string>("--app-id", "Id of the app. Used during installation in many installers.");
            var appTypeOption = new Option<AppType>("--app-type", "Specifies the installer to use.");
            var environmentsOption = new Option<List<string>>(name: "--environments", 
                parseArgument: x =>
                    x.Tokens.Select(y => new Token?(y)).FirstOrDefault()?.Value
                        .Split("|", StringSplitOptions.RemoveEmptyEntries).ToList()
                    ?? new List<string>(),
                description: "Specifies which environments this app should install for.");

            var addAppCommand = new Command("add-app", "Add app for use on the next machine.")
            {
                appIdOption,
                appTypeOption,
                environmentsOption
            };
            
            addAppCommand.AddAlias("add-app");
            addAppCommand.AddAlias("add");

            addAppCommand.SetHandler<string, AppType, List<string>>(async (appId, appType, environments) =>
            {
                var services = await dependencyBootstrapper.InitializeAsync();
                await services.GetRequiredService<IAddAppCommand>().ExecuteAsync(appId, appType, environments);
            }, appIdOption, appTypeOption, environmentsOption);

            return addAppCommand;
        }

        private Command CreateBackupCommand()
        {
            var backupCommand = new Command("backup", "Backup app configurations etc. for use on the next machine.");
            backupCommand.SetHandler(async () =>
            {
                var services = await dependencyBootstrapper.InitializeAsync();
                await services.GetRequiredService<IBackupMachineCommand>().ExecuteAsync();
            });

            return backupCommand;
        }

        private Command CreateSettingsCommand()
        {
            var settingsCommand = new Command("settings", "Manage settings.")
            {
                CreateListSettingsCommand(),
                CreateSetSettingCommand()
            };

            return settingsCommand;
        }

        private Command CreateListSettingsCommand()
        {
            var listSettingsCommand = new Command("list", "List all settings with values.");

            listSettingsCommand.SetHandler(async () =>
            {
                var services = await dependencyBootstrapper.InitializeAsync();
                await services.GetRequiredService<IListSettingsCommand>().ExecuteAsync();
            });

            return listSettingsCommand;
        }

        private Command CreateSetSettingCommand()
        {
            var settingNameArg = new Argument<string>("setting-name", "Name of the setting to change.");
            var settingValueArg = new Argument<string>("setting-value", "New setting value.");

            var setSettingCommand = new Command("set", "Set single named setting.")
            {
                settingNameArg,
                settingValueArg
            };

            setSettingCommand.SetHandler<string, string>(async (settingName, settingValue) =>
            {
                var services = await dependencyBootstrapper.InitializeAsync();
                await services.GetRequiredService<ISetSettingCommand>().ExecuteAsync(settingName, settingValue);
            }, settingNameArg, settingValueArg);

            return setSettingCommand;
        }

        private Command CreateInitializeCommand()
        {
            var initializeCommand = new Command("initialize", "Runs system initialization and clones the manifest repo in settings.");
            
            initializeCommand.SetHandler(async () =>
            {
                var services = await dependencyBootstrapper.InitializeAsync();
                await services.GetRequiredService<IInitializeCommand>().ExecuteAsync();
            });

            return initializeCommand;
        }

        private Command CreateConfigureMachineCommand()
        {
            var environmentsOption = new Option<List<string>>(
                aliases: new[] { "--environments", "-e" },
                parseArgument: x =>
                    x.Tokens.Select(y => new Token?(y)).FirstOrDefault()?.Value
                        .Split("|", StringSplitOptions.RemoveEmptyEntries).ToList()
                        ?? new List<string>(),
                isDefault: true,
                description: "Pipe separated list of environments to target in the manifest.");
            var singleAppOption = new Option<string>(
                aliases: new[] { "--single-app-id", "-app" },
                description: "The single app to install by Id. When present the environments arg is ignored.");

            var configureMachineCommand = new Command("configure-machine", "Runs all apps of the manifest repo in settings.")
            {
                environmentsOption,
                singleAppOption
            };

            configureMachineCommand.AddAlias("configure");

            configureMachineCommand.SetHandler<List<string>, string>(async (environments, singleAppId) =>
            {
                var services = await dependencyBootstrapper.InitializeAsync();
                await services.GetRequiredService<IConfigureMachineCommand>().ExecuteAsync(environments, singleAppId);
            }, environmentsOption, singleAppOption);

            return configureMachineCommand;
        }
    }
}
