using System;
using System.Reflection;
using System.Threading.Tasks;
using Configurator.Configuration;
using Configurator.Utilities;
using Configurator.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Configurator
{
    public interface IDependencyBootstrapper
    {
        Task<IServiceProvider> InitializeAsync();
    }

    public class DependencyBootstrapper : IDependencyBootstrapper
    {
        private readonly IServiceCollection serviceCollection;

        public DependencyBootstrapper(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public async Task<IServiceProvider> InitializeAsync()
        {
            var serviceProvider = InitializeServiceProvider();
            InitializeStaticDependencies(serviceProvider);

            WriteDependencyDebugInfo(serviceProvider);

            return serviceProvider;
        }

        /// <summary>
        /// Not for public consumption! Only exposed for unit testing!
        /// </summary>
        internal ServiceProvider InitializeServiceProvider()
        {
            DependencyInjectionConfig.ConfigureServices(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Not for public consumption! Only exposed for unit testing!
        /// </summary>
        internal void InitializeStaticDependencies(IServiceProvider services)
        {
            RegistrySettingValueDataConverter.Tokenizer = services.GetRequiredService<ITokenizer>();
        }

        private static void WriteDependencyDebugInfo(IServiceProvider services)
        {
            var logger = services.GetRequiredService<IConsoleLogger>();

            var version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly())
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
                .InformationalVersion;
            logger.Debug($"{nameof(Configurator)} version: {version}");
        }
    }
}
