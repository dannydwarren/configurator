using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Configurator
{
    public class Cli
    {
        private readonly IDependencyBootstrapper dependencyBootstrapper;
        private readonly IConsoleLogger consoleLogger;

        public Cli(IDependencyBootstrapper dependencyBootstrapper, IConsoleLogger consoleLogger)
        {
            this.dependencyBootstrapper = dependencyBootstrapper;
            this.consoleLogger = consoleLogger;
        }

        public async Task<int> LaunchAsync(params string[] args)
        {
            var rootCommand = CreateRootCommand();

            rootCommand.Add(CreateSettingsCommand());
            rootCommand.Add(CreateBackupCommand());
            
            return await rootCommand.InvokeAsync(args);
        }

        private RootCommand CreateRootCommand()
        {
            var manifestPath = new Option<string>(
                aliases: new[] { "--manifest-path", "-m" },
                getDefaultValue: () => Arguments.Default.ManifestPath,
                description: "Path (local or URL) to your manifest file.");
            var environments = new Option<List<string>>(
                aliases: new[] { "--environments", "-e" },
                parseArgument: x =>
                    x.Tokens.Select(y => new Token?(y)).FirstOrDefault()?.Value
                        .Split("|", StringSplitOptions.RemoveEmptyEntries).ToList()
                    ?? Arguments.Default.Environments,
                isDefault: true,
                description: "Pipe separated list of environments to target in the manifest.");
            var downloadsDir = new Option<string>(
                aliases: new[] { "--downloads-dir", "-dl" },
                getDefaultValue: () => Arguments.Default.DownloadsDir,
                description: "Local path to use for downloads.");
            var singleApp = new Option<string>(
                aliases: new[] { "--single-app-id", "-app" },
                description: "The single app to install by Id. When present the environments arg is ignored.");

            var rootCommand = new RootCommand("Configurator")
            {
                manifestPath,
                environments,
                downloadsDir,
                singleApp,
            };

            rootCommand.SetHandler<string, List<string>, string, string>(RunConfiguratorAsync,
                manifestPath, environments, downloadsDir, singleApp);
            
            return rootCommand;
        }

        private async Task RunConfiguratorAsync(string manifestPath, List<string> environments, string downloadsDir,
            string? singleAppId)
        {
            var singleAppIdCoalesced = string.IsNullOrWhiteSpace(singleAppId) ? null : singleAppId;

            var arguments = new Arguments(manifestPath, environments, downloadsDir, singleAppIdCoalesced);

            var services = await dependencyBootstrapper.InitializeAsync(arguments);
            var configurator = services.GetRequiredService<IMachineConfigurator>();

            await configurator.ExecuteAsync();
        }

        private Command CreateBackupCommand()
        {
            var backupCommand = new Command("backup", "Backup app configurations etc. for use on the next machine.");
            backupCommand.SetHandler(() => consoleLogger.Debug("Support for backing up apps is in progress..."));
            
            return backupCommand;
        }

        private Command CreateSettingsCommand()
        {
            var settingName = new Argument<string>("setting-name", "Name of the setting to change.");
            var settingValue = new Argument<string>("setting-value", "New setting value.");
            
            var settingsCommand = new Command("settings", "Set CLI configuration values.")
            {
                settingName,
                settingValue
            };
            
            settingsCommand.SetHandler<string, string>(RunSettingsAsync, settingName, settingValue);
            
            return settingsCommand;
        }

        private async Task RunSettingsAsync(string settingName, string settingValue)
        {
            var services = await dependencyBootstrapper.InitializeAsync(Arguments.Default);
            var updateSettingsCommand = services.GetRequiredService<IUpdateSettingsCommand>();
            
            await updateSettingsCommand.ExecuteAsync(settingName, settingValue);
        }
    }
}
