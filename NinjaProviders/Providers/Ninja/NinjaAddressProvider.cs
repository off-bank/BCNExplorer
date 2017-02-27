﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using NBitcoin;
using Newtonsoft.Json;
using Providers.BlockChainReader;
using Providers.Contracts.Ninja;
using Providers.TransportTypes.Ninja;

namespace Providers.Providers.Ninja
{
    public class NinjaAddressAliases
    {
        public string ColoredAddress { get; set; }
        public string UncoloredAddress { get; set; }
    }

    public class NinjaAddressSummary
    {
        public NinjaAddressBalance Confirmed { get; set; }
        
        public NinjaAddressBalance Unconfirmed { get; set; }

        public class NinjaAddressBalance
        {
            public NinjaAddressBalance()
            {
                Assets = Enumerable.Empty<NinjaAddressAssetSummary>().ToList();
            }

            public int TotalTransactions { get; set; }

            public long Balance { get; set; }

            public IList<NinjaAddressAssetSummary> Assets { get; set; }

            public class NinjaAddressAssetSummary
            {
                public string AssetId { get; set; }
                public long Quantity { get; set; }
                public long Received { get; set; }
            }

            public static NinjaAddressBalance Create(AddressSummaryContract.AddressSummaryInnerContract source)
            {
                if (source != null)
                {
                    return new NinjaAddressBalance
                    {
                        Balance = source.Balance,
                        TotalTransactions = source.TotalTransactions,
                        Assets = (source.Assets ?? Enumerable.Empty<AddressSummaryContract.AddressSummaryInnerContract.AddressAssetContract>())
                           .Select(p => new NinjaAddressAssetSummary
                           {
                               AssetId = p.AssetId,
                               Quantity = p.Quantity,
                               Received = p.Received
                           }).ToList()
                    };
                }

                return null;
            }
        }
    }
    
    public class NinjaAddressTransactionList
    {
        public NinjaAddressTransactionList()
        {
            AllTransactions = Enumerable.Empty<NinjaAddressTransaction>();
        }

        public IEnumerable<NinjaAddressTransaction> AllTransactions { get; set; }
        
        public class NinjaAddressTransaction
        {
            public string TxId { get; set; }
            public bool IsReceived { get; set; }
            public int BlockHeight { get; set; }
            public string BlockHash { get; set; }

            public static NinjaAddressTransaction Create(AddressTransactionListItemContract source)
            {
                return new NinjaAddressTransaction
                {
                    TxId = source.TxId,
                    IsReceived = source.IsReceived(),
                    BlockHeight = source.Height,
                    BlockHash = source.BlockId
                };
            }
        }
    }

    public class NinjaAddressProvider
    {
        private readonly NinjaBlockChainReader _blockChainReader;

        public NinjaAddressProvider(NinjaBlockChainReader blockChainReader)
        {
            _blockChainReader = blockChainReader;
        }
        
        public async Task<NinjaAddressAliases> GetAliases(string id)
        {
            var result = await _blockChainReader.GetAsync<WhatIsItAdrressContract>($"/whatisit/{id}");

            if (result != null)
            {
                return new NinjaAddressAliases
                {
                    ColoredAddress = result.ColoredAddress,
                    UncoloredAddress = result.UncoloredAddress
                };
            }

            return null;
        }

        public async Task<NinjaAddressTransactionList> GetTransactionsForAddressAsync(string id, int? until = null, int? from = null)
        {
            var url = $"/balances/{id}";

            if (until != null || from != null)
            {
                var queryParams = new
                {
                    until = until,
                    from = from
                };
                
                url += "?" + queryParams.ToUrlParamString();
            }

            var result =  await _blockChainReader.GetAsync<AddressTransactionListContract>(url);

            if (result != null)
            {
                var tx = result.Transactions ?? Enumerable.Empty<AddressTransactionListItemContract>();
                return new NinjaAddressTransactionList
                {
                    AllTransactions = tx.Select(NinjaAddressTransactionList.NinjaAddressTransaction.Create)
                };
            }

            return null;
        }



        public async Task<NinjaAddressSummary> GetAddressBalanceAsync(string id, int? at, bool colored)
        {
            if (!colored)
            {
                //getting uncolored address if id is colored
                try
                {
                    id = new BitcoinColoredAddress(id).Address.ToWif();
                }
                catch (Exception)
                {
                    //address is uncolored already - its ok
                }
            }
            var url = $"/balances/{id}/summary?colored={colored}";
            if (at != null)
            {
                url += $"&at={at.Value}";
            }

            var result = await _blockChainReader.GetAsync<AddressSummaryContract>(url);

            if (result != null)
            {
                return new NinjaAddressSummary
                {
                    Confirmed = NinjaAddressSummary.NinjaAddressBalance.Create(result.Confirmed),
                    Unconfirmed = NinjaAddressSummary.NinjaAddressBalance.Create(result.Unconfirmed)
                };
            }

            return null;
        }
    }

    static class AddressTransactionListItemContractHelper
    {
        public static bool IsSend(this AddressTransactionListItemContract source)
        {
            if (source.Amount < 0 || (source.Amount==0 &&source.Spent.Any()))
            {
                return true;
            }

            return false;
        }

        public static bool IsReceived(this AddressTransactionListItemContract source)
        {
            if (source.Amount > 0  || (source.Amount == 0 && source.Received.Any()))
            {
                return true;
            }

            return false;
        }
    }


}

