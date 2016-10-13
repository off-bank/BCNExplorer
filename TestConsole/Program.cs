﻿using AzureRepositories;
using AzureRepositories.Binders;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Providers;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(JobsConnectionStringSettings.ConnectionString);
            var logToTable = new LogToConsole();



        }

        private static void InitContainer(IoC container, BaseSettings settings, ILog log)
        {
            container.Register<ILog>(log);

            container.BindProviders(settings, log);
            container.Register(settings);
            container.BindAzureRepositories(settings, log);
        }
    }
}
