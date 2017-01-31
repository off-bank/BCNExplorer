﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.BalanceReport;
using Providers.Providers.Lykke.API;

namespace Services.BalanceChanges
{
    public class FiatRatesService
    {
        private readonly LykkeAPIProvider _lykkeApiProvider;

        public FiatRatesService(LykkeAPIProvider lykkeApiProvider)
        {
            _lykkeApiProvider = lykkeApiProvider;
        }

        public async Task<IEnumerable<IFiatRates>> GetRatesAsync(DateTime at, IEnumerable<string> quotingCurrencies, IEnumerable<string> btcAssetIds)
        {
            var assets = await _lykkeApiProvider.GetAssetsAsync();
            var pairs = await _lykkeApiProvider.GetAssetPairDictionary();

            var baseAssetIds = assets.Where(p => btcAssetIds.Contains(p.BitcoinAssetId) || p.Name == "BTC").Select(p => p.Id).ToList();

            var assetPairs = pairs.Where(p => quotingCurrencies.Contains(p.QuotingAssetId) && baseAssetIds.Contains(p.BaseAssetId));
            
            var rates = (await _lykkeApiProvider.GetAssetPairRatesAtTimeAsync(at, AssetPairRateHistoryPeriod.Day,
                assetPairs.Select(p => p.Id).ToArray())).ToDictionary(p => p.Id, p => p.Rate);

            var result = new List<FiatRate>();
            foreach (var currency in quotingCurrencies)
            {
                var relatedPairs = pairs.Where(p => p.QuotingAssetId == currency);
                var priceDictionary = new Dictionary<string, decimal>();

                var btcCurrencyAsset = assets.FirstOrDefault(p => p.Name == currency);
                if (btcCurrencyAsset != null)
                {
                    priceDictionary[btcCurrencyAsset.BitcoinAssetId] = 1;
                }
                     
                foreach (var pair in relatedPairs)
                {
                    if (rates.ContainsKey(pair.Id))
                    {
                        var baseAsset = assets.FirstOrDefault(p => p.Id == pair.BaseAssetId);

                        if (baseAsset != null)
                        {
                            priceDictionary[baseAsset.BitcoinAssetId] = rates[pair.Id];
                        }

                        priceDictionary[pair.BaseAssetId] = rates[pair.Id];
                    }
                }

                result.Add(FiatRate.Create(currency, priceDictionary));
            }

            return result;
        } 
    }
}
