﻿using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Configurator.Utilities;
using Configurator.Windows;

namespace Configurator
{
    internal static class Program
    {
        internal static async Task<int> Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            var dependencyBootstrapper = new DependencyBootstrapper(serviceCollection);
            var cli = new Cli(dependencyBootstrapper, new PrivilegesRepository(), new ConsoleLogger());

            return await cli.LaunchAsync(args);
        }
    }
}
