﻿using AzureRepositories;
using AzureRepositories.Binders;
using BalanceReporting.Binders;
using Common.IocContainer;
using Common.Log;
using Core.Settings;
using JobsCommon;
using Providers;
using Providers.Binders;
using Services.Binders;
using TestConsole.AssetScanner;
using TestConsole.BalanceReport;
using TestConsole.Coninholders;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = GeneralSettingsReader.ReadGeneralSettings<BaseSettings>(JobsConnectionStringSettings.ConnectionString);
     
            var container = new IoC();
            InitContainer(container, settings, new LogToConsole());

            //TestGettingChainChanges.Run(container);
            //GetAddressesWithColoredAssetsFromCoinPrism.Run(container);
            //TestFillBalance.Run(container).Wait();
            // CheckBalance.Run(container).Wait();
            //MongoFillBalance.Run(container).Wait();
            //AddAddressesFromBlockChain.Run(container).Wait();
            //AssetDefToCsv.Run(container).Wait();
            //AssetCoinholdersToCsv.Run(container).Wait();
            //AddMissingDefinitions.Run(container).Wait();
            //AddAssetDefinitionsHistory.Run(container).Wait();
            //CopyMongoData.Run(container).Wait();
            //Pdf.Run(container).Wait();
            ParseBlock.Run(container).Wait();
        }

        private static void InitContainer(IoC container, BaseSettings settings, ILog log)
        {
            settings.CacheMainChainLocalFile = true;
            settings.DisablePersistentCacheMainChain = true;
            settings.ExplolerUrl = "https://blockchainexplorer.lykke.com/";
            container.Register<ILog>(log);
            container.BindBalanceReportingFunctions(settings, log);

            container.BindProviders(settings, log);
            container.Register(settings);
            container.BindAzureRepositories(settings, log);
            container.BindServices(settings, log);
        }

    }
}
