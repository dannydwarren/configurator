﻿using System;
using Configurator.Apps;
using Configurator.Utilities;
using Configurator.Windows;

namespace Configurator.Installers
{
    public interface IAppConfigurator
    {
        void Configure(IApp app);
    }

    public class AppConfigurator : IAppConfigurator
    {
        private readonly IRegistryRepository registryRepository;
        private readonly IConsoleLogger logger;

        public AppConfigurator(IRegistryRepository registryRepository, IConsoleLogger logger)
        {
            this.registryRepository = registryRepository;
            this.logger = logger;
        }

        public void Configure(IApp app)
        {
            if (app.Configuration == null)
                return;

            app.Configuration.RegistrySettings.ForEach(setting =>
            {
                try
                {
                    registryRepository.SetValue(setting.KeyName, setting.ValueName, setting.ValueData);
                }
                catch (Exception e)
                {
                    logger.Error($"Error setting value in registry >> {nameof(setting.KeyName)}: {setting.KeyName}; {nameof(setting.ValueName)}: {setting.ValueName}", e);
                    throw;
                }
            });
        }
    }
}
