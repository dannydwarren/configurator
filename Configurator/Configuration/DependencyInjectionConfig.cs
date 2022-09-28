using System.Runtime.CompilerServices;
using Configurator.Installers;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Configurator.UnitTests")]
[assembly: InternalsVisibleTo("Configurator.IntegrationTests")]
namespace Configurator.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.Scan(
                scan =>
                {
                    scan.FromAssembliesOf(typeof(DependencyInjectionConfig))
                        .AddClasses()
                        .AsSelf()
                        .AsMatchingInterface()
                        .WithTransientLifetime();
                }
            );

            services.AddTransient<IAppInstallerForceWindowsPowerShell, AppInstaller>();

            Emmersion.Http.DependencyInjectionConfig.ConfigureServices(services);
        }
    }
}
