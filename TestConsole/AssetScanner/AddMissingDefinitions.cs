﻿using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using AzureRepositories.AssetDefinition;
using AzureStorage.Tables;
using Common;
using Common.IocContainer;
using Core.Settings;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestConsole.AssetScanner
{
    public static class AddMissingDefinitions
    {
        public static async Task Run(IoC container)
        {
            Console.WriteLine("AddMissingDefinitions started");
            
            var sourceConnectionString = ConfigurationManager.AppSettings["sourceConnectionString"];
            var targetConnectionString = ConfigurationManager.AppSettings["targetConnectionString"];
            var tableName = "AssetDefinitions";

            var sourceRepo = new AzureTableStorage<AssetDefinitionDefinitionEntity>(sourceConnectionString, tableName, null);
            var targetRepo = new AzureTableStorage<AssetDefinitionDefinitionEntity>(targetConnectionString, tableName, null);

            var sourceAll = sourceRepo.GetData(p => p.PartitionKey == AssetDefinitionDefinitionEntity.GeneratePartitionKey()).ToDictionary(p=>p.RowKey);
            var targetAll = targetRepo.GetData(p => p.PartitionKey == AssetDefinitionDefinitionEntity.GeneratePartitionKey()).ToDictionary(p => p.RowKey);

            var entitiesToAdd = sourceAll.Where(p => !targetAll.ContainsKey(p.Key));

            Console.WriteLine(entitiesToAdd.Count());
            await targetRepo.InsertAsync(entitiesToAdd.Select(p=>p.Value));


            Console.WriteLine("AddMissingDefinitions done");
            Console.ReadLine();
        }
    }
}
