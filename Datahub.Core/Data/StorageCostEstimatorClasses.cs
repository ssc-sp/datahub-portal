using System;
using System.Collections.Generic;

namespace Datahub.Core.Data.StorageCostEstimator
{
    public record class EstimatorResultLine(int NumberOfOperations, decimal Cost);
    public class EstimatorResult
    {
        public EstimatorResultLine WriteOperations { get; set; }
        public EstimatorResultLine ListCreateOperations { get; set; }
        public EstimatorResultLine ReadOperations { get; set; }
        public EstimatorResultLine OtherOperations { get; set; }
        public EstimatorResultLine DataRetrieval { get; set; }
        public EstimatorResultLine DataWrite { get; set; }
        public EstimatorResultLine GeoReplication { get; set; }

        private decimal Cost(EstimatorResultLine l) => l?.Cost ?? 0.0000M;
        public bool HasValues => (WriteOperations ?? ListCreateOperations ?? ReadOperations ?? OtherOperations ?? DataRetrieval ?? DataWrite ?? GeoReplication) != null;
        public decimal TotalCost => Cost(WriteOperations) + Cost(ListCreateOperations) + Cost(ReadOperations) +
            Cost(OtherOperations) + Cost(DataRetrieval) + Cost(DataWrite) + Cost(GeoReplication);
    }

    public record class UnitPrice(decimal BasePrice, int Units);
    
    public class EstimatorPriceList
    {
        public UnitPrice Capacity { get; set; }
        public UnitPrice WriteOperations { get; set; }
        public UnitPrice ListCreateOperations { get; set; }
        public UnitPrice ReadOperations { get; set; }
        public UnitPrice ArchiveHPRead { get; set; }
        public UnitPrice DataRetrieval { get; set; }
        public UnitPrice DataWrite { get; set; }
        public UnitPrice OtherOperations { get; set; }
        public UnitPrice GeoReplication { get; set; }
    }


    public enum AccessTierType
    {
        Hot,
        Cool,
        Archive
    }

    public enum DataRedundancyType
    {
        LRS,
        ZRS,
        GRS
    }

    public class AzurePriceAPIItem
    {
        public string MeterId { get; set; }
        public string MeterName { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string SkuId { get; set; }
        public string SkuName { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string ServiceFamily { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public string ArmSkuName { get; set; }
        public string ArmRegionName { get; set; }
        public string Location { get; set; }
        public string CurrencyCode { get; set; }
        public decimal TierMinimumUnits { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public bool IsPrimaryMeterRegion { get; set; }
        public string Type { get; set; }
    }

    public interface IAzurePriceAPIItemContainer
    {
        IList<AzurePriceAPIItem> Items { get; set; }
    }

    public class AzurePriceAPIResult: IAzurePriceAPIItemContainer
    {
        public string BillingCurrency { get; set; }
        public int Count { get; set; }
        public string NextPageLink { get; set; }
        public string CustomerEntityType { get; set; }
        public string CustomerEntityId { get; set; }
        public IList<AzurePriceAPIItem> Items { get; set; }
    }

    public class AzurePriceAPIItemList : IAzurePriceAPIItemContainer
    {
        public IList<AzurePriceAPIItem> Items { get; set; }
        public AzurePriceAPIItemList(IList<AzurePriceAPIItem> items)
        {
            Items = items;
        }
    }

    public class SavedStorageCostPriceGrid
    {
        public DateTime LastUpdatedUtc { get; set; }
        
        public Dictionary<string, EstimatorPriceList> PriceLists { get; set; }
    }

    public class MiscStoredObject
    {
        public string Id { get; set; }
        public string TypeName { get; set; }
        public string JsonContent { get; set; }
    }


}
