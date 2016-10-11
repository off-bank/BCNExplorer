﻿using System.Linq;
using System.Threading.Tasks;
using AzureRepositories.Asset;
using Common;
using Common.Log;
using Core.Asset;
using Microsoft.Azure.WebJobs;

namespace AssetScanner.Functions
{
    public class UpdateAssetDataFunctions
    {
        private readonly IAssetDefinitionRepository _assetDefinitionRepository;
        private readonly ILog _log;
        private readonly AssetDataCommandProducer _assetDataCommandProducer;

        public UpdateAssetDataFunctions(IAssetDefinitionRepository assetDefinitionRepository, ILog log,  AssetDataCommandProducer assetDataCommandProducer)
        {
            _assetDefinitionRepository = assetDefinitionRepository;
            _log = log;
            _assetDataCommandProducer = assetDataCommandProducer;
        }

        public async Task UpdateAssets([TimerTrigger("00:30:00", RunOnStartup = true)] TimerInfo timer)
        {
            var assetsToUpdate = await _assetDefinitionRepository.GetAllAsync();

            await _log.WriteInfo("AssetUpdaterFunctions", "UpdateAssets", assetsToUpdate.ToJson(), "Update assets started");

            await _assetDataCommandProducer.CreateUpdateAssetDataCommand(
                    assetsToUpdate.Select(p => p.AssetDefinitionUrl).ToArray());
        }
    }
}
