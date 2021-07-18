﻿using Configurator.PowerShell;
using Microsoft.Extensions.DependencyInjection;

namespace Configurator.Configuration
{
    public static class DependencyInjectionConfig
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.Scan(
                scan =>
                {
                    scan.FromAssembliesOf(typeof(DependencyInjectionConfig)).AddClasses().AsMatchingInterface().WithTransientLifetime();
                }
            );

            services.AddSingleton<IPowerShellConnection>(new PowerShellConnection());
        }
    }
}
