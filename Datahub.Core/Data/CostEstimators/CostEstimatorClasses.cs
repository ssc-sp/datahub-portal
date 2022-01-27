using System;
using System.Collections.Generic;

namespace Datahub.Core.Data.CostEstimator
{
    public record class EstimatorResultLine(int NumberOfOperations, decimal Cost);
    public record class EstimatorResultLineDecimal(decimal NumberOfOperations, decimal Cost);
    
    public record class UnitPrice(decimal BasePrice, int Units);
    
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

}
