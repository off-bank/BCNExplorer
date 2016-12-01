﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Core.Asset;
using Core.Transaction;

namespace BCNExplorer.Web.Models
{
    public class TransactionViewModel
    {
        public string TransactionId { get; set; }
        public bool IsColor { get; set; }
        public bool IsCoinBase { get; set; }
        public int AssetsCount { get; set; }
        public bool IsConfirmed { get; set; }
        public BlockViewModel Block { get; set; }
        public BitcoinAsset Bitcoin { get; set; }
        public IEnumerable<ColoredAsset> ColoredAssets { get; set; }
        
        #region Classes 

        public class BlockViewModel
        {
            public double Confirmations { get; set; }

            public DateTime Time { get; set; }
     
            public double Height { get; set; }
            public string BlockId { get; set; }

            public static BlockViewModel Create(IBlockMinInfo blockMinInfo)
            {
                if (blockMinInfo != null)
                {
                    return new BlockViewModel
                    {
                        Confirmations = blockMinInfo.Confirmations,
                        Height = blockMinInfo.Height,
                        Time = blockMinInfo.Time,
                        BlockId = blockMinInfo.BlockId
                    };
                }

                return null;
            }
        }

        public class BitcoinAsset
        {
            public bool IsCoinBase { get; set; }
            
            public IEnumerable<AggregatedInOut<In>> AggregatedIns { get; set; }
            public IEnumerable<AggregatedInOut<Out>> AggregatedOuts { get; set; }

            public double Fees { get; set; }
            public string FeesDescription => Fees.ToStringBtcFormat();

            public static IEnumerable<In> Group(IEnumerable<In> source)
            {
                var groupedByAddress = source.GroupBy(p => p.Address);
                return groupedByAddress.Select(p =>
                {
                    var result = p.First();
                    result.Value = p.Sum(x => x.Value);

                    return result;
                });
            } 

            public static BitcoinAsset Create(double fees,
                bool isCoinBase,
                IEnumerable<IInOut> ninjaIn,
                IEnumerable<IInOut> ninjaOuts)
            {
                var ins = In.Create(ninjaIn);
                var outs = Out.Create(ninjaOuts);

                return new BitcoinAsset
                {
                    Fees = BitcoinUtils.SatoshiToBtc(fees),
                    IsCoinBase = isCoinBase,
                    AggregatedIns = AssetHelper.GroupByAddress(ins),
                    AggregatedOuts = AssetHelper.GroupByAddress(outs)
                };
            }

            public class In:AssetInOut
            {
                public In(double value, string address, string previousTransactionId)
                {
                    Value = value;
                    Address = address;
                    PreviousTransactionId = previousTransactionId;
                }

                public override string PreviousTransactionId { get; }

                public override string ValueDescription => Value.ToStringBtcFormat();
                
                public override int AggregatedTransactionCount => _aggregatedTransactionCount;
                
                public static IEnumerable<In> Create(IEnumerable<IInOut> ins)
                {
                    return ins.Where(p => p.Value != 0)
                        .Select(p => new In(
                            value: BitcoinUtils.SatoshiToBtc(p.Value * (-1)), 
                            address:p.Address, 
                            previousTransactionId:p.TransactionId));
                }
            }

            public class Out:AssetInOut
            {
                public Out(double value, string address)
                {
                    Value = value;
                    Address = address;
                }

                public override string PreviousTransactionId => null;

                public override string ValueDescription => Value.ToStringBtcFormat();
                
                public override int AggregatedTransactionCount => _aggregatedTransactionCount;
                
                public static IEnumerable<Out> Create(IEnumerable<IInOut> outs)
                {
                    return outs.Where(p => p.Value != 0).Select(p => new Out(value: BitcoinUtils.SatoshiToBtc(p.Value), address:p.Address));
                }
            }
        }

        public class ColoredAsset
        {
            public string AssetId { get; set; }
            public int Divisibility { get; set; }
            public string Name { get; set; }
            public string IconImageUrl { get; set; }

            public IEnumerable<AggregatedInOut<In>> AggregatedIns { get; set; }
            public IEnumerable<AggregatedInOut<Out>> AggregatedOuts { get; set; }

            #region Classes

            public class In:AssetInOut
            {
                public In(double value, string address, string previousTransactionId, string shortName)
                {
                    Value = value;
                    Address = address;
                    PreviousTransactionId = previousTransactionId;
                    ShortName = shortName;
                }

                public override string PreviousTransactionId { get; }

                public override string ValueDescription => $"{Value.ToStringBtcFormat()} {ShortName}";
                
                public override int AggregatedTransactionCount => _aggregatedTransactionCount;

                public string ShortName { get; set; }

                public static In Create(IInOut sourceIn, int divisibility,  IEnumerable<IInOut> outs, string shortName)
                {
                    //var l = outs.Select(itm => itm.Quantity - sourceIn.Quantity);
                    //var def = l.FirstOrDefault();
                    //var quantity = (def != 0 ? def : sourceIn.Quantity)*(-1);

                    return new In(
                        value: BitcoinUtils.CalculateColoredAssetQuantity(sourceIn.Quantity *(-1), divisibility), 
                        address: sourceIn.Address, 
                        previousTransactionId: sourceIn.TransactionId,
                        shortName: shortName);
                }
            }

            public class Out:AssetInOut
            {
                public Out(double value, string address, string shortName)
                {
                    Value = value;
                    Address = address;
                    ShortName = shortName;
                }

                public override string PreviousTransactionId => null;

                public override string ValueDescription => $"{Value.ToStringBtcFormat()} {ShortName}";


                public override int AggregatedTransactionCount => _aggregatedTransactionCount;
                

                public string ShortName { get; set; }

                public static Out Create(IInOut sourceOut, int divisibility, string shortName)
                {
                    return new Out(
                        value: BitcoinUtils.CalculateColoredAssetQuantity(sourceOut.Quantity, divisibility), 
                        address: sourceOut.Address,
                        shortName: shortName);
                }
            }
            
            #endregion

            public static ColoredAsset Create(IInOutsByAsset inOutsByAsset, AssetDictionary assetDictionary)
            {
                var divisibility = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Divisibility, 0);
                var assetShortName = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.NameShort, null);

                var ins = inOutsByAsset.TransactionIn.Select(p => In.Create(p, divisibility, inOutsByAsset.TransactionsOut, assetShortName));
                var outs = inOutsByAsset.TransactionsOut.Select(p => Out.Create(p, divisibility, assetShortName));
                
                var result = new ColoredAsset
                {
                    AssetId = inOutsByAsset.AssetId,
                    Divisibility = divisibility,
                    Name = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.Name, null),
                    IconImageUrl = assetDictionary.GetAssetProp(inOutsByAsset.AssetId, p => p.IconUrl, null),
                    AggregatedIns = AssetHelper.GroupByAddress(ins),
                    AggregatedOuts = AssetHelper.GroupByAddress(outs)
                };

                return result;
            }
        }

        #endregion

        public static TransactionViewModel Create(ITransaction ninjaTransaction, IDictionary<string, IAsset> assetDictionary)
        {
            var bc = ninjaTransaction.TransactionsByAssets.First(p => !p.IsColored);
            var colored = ninjaTransaction.TransactionsByAssets.Where(p => p.IsColored).OrderBy(p => p.AssetId);
            var assetDic = AssetDictionary.Create(assetDictionary);

            var result = new TransactionViewModel
            {
                TransactionId = ninjaTransaction.TransactionId,
                IsCoinBase = ninjaTransaction.IsCoinBase,
                IsColor = ninjaTransaction.IsColor,
                Block = BlockViewModel.Create(ninjaTransaction.Block),
                AssetsCount = ninjaTransaction.TransactionsByAssets.Count(p => p.IsColored),
                IsConfirmed = ninjaTransaction.Block != null,
                Bitcoin = BitcoinAsset.Create(ninjaTransaction.Fees, ninjaTransaction.IsCoinBase, bc.TransactionIn.Union(colored.SelectMany(p => p.TransactionIn)), bc.TransactionsOut.Union(colored.SelectMany(p=>p.TransactionsOut)) ),
                ColoredAssets = colored.Select(p => ColoredAsset.Create(p, assetDic))
            };
            
            return result;
        }
    }

    public abstract class AssetInOut: IInOutViewModel
    {
        public string Address { get; set; }
        public double Value { get; set; }

        public abstract string PreviousTransactionId { get; }
        public bool ShowPreviousTransaction => PreviousTransactionId != null;
        public abstract string ValueDescription { get; }
        public bool ShowAggregatedTransactions => AggregatedTransactionCount > 1;
        public abstract int AggregatedTransactionCount { get; }

        protected int _aggregatedTransactionCount;
        public virtual void SetAggregatedTransactionCount(int count)
        {
            _aggregatedTransactionCount = count;
        }

        public bool IsUnrecoginzedAddress => string.IsNullOrEmpty(Address);

        public virtual T Clone<T>() where T:AssetInOut
        {
            return (T)this.MemberwiseClone();
        }
    }

    public class AggregatedInOut<T> where T: AssetInOut
    {
        public T TitleItem { get; set; }

        public IEnumerable<T> AggregatedTransactions { get; set; }

        public bool ShowAggregatedTransactions => AggregatedTransactions?.Count() > 1;

        public int Count => AggregatedTransactions?.Count() ?? 0;
    }

    public static class AssetHelper
    {
        public static IEnumerable<AggregatedInOut<T>> GroupByAddress<T>(IEnumerable<T> source) where T: AssetInOut
        {
            return source
                .GroupBy(p => p.Address)
                .Select(p =>
                {
                    var titleItem = p.First().Clone<T>();
                    var allItems = p.ToList();

                    titleItem.Value = p.Sum(ti => ti.Value);
                    titleItem.SetAggregatedTransactionCount(allItems.Count);
                    return new AggregatedInOut<T>
                    {
                        TitleItem = titleItem,
                        AggregatedTransactions = allItems
                    };
                });
        } 
    }

    public interface IInOutViewModel
    {
        string ValueDescription { get; } 
        bool ShowAggregatedTransactions { get;  }
        int AggregatedTransactionCount { get;  }
        bool IsUnrecoginzedAddress { get; }
        string Address { get; }
        bool ShowPreviousTransaction { get; }
        string PreviousTransactionId { get; }
    }
}