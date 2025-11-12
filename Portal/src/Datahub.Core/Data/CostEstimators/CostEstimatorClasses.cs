namespace Datahub.Core.Data.CostEstimators;

public record class EstimatorResultLine(int NumberOfOperations, decimal Cost);
public record class EstimatorResultLineDecimal(decimal NumberOfOperations, decimal Cost);

public record class UnitPrice(decimal BasePrice, int Units);

public class AzurePriceAPIItem
{
    public string MeterId { get; set; } = null!;
    public string MeterName { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string SkuId { get; set; } = null!;
    public string SkuName { get; set; } = null!;
    public string ServiceId { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string ServiceFamily { get; set; } = null!;
    public string UnitOfMeasure { get; set; } = null!;
    public decimal UnitPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public string ArmSkuName { get; set; } = null!;
    public string ArmRegionName { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string CurrencyCode { get; set; } = null!;
    public decimal TierMinimumUnits { get; set; }
    public DateTime EffectiveStartDate { get; set; }
    public bool IsPrimaryMeterRegion { get; set; }
    public string Type { get; set; } = null!;
}

public interface IAzurePriceAPIItemContainer
{
    IList<AzurePriceAPIItem> Items { get; set; }
}

public class AzurePriceAPIResult : IAzurePriceAPIItemContainer
{
    public string BillingCurrency { get; set; } = null!;
    public int Count { get; set; }
    public string NextPageLink { get; set; } = null!;
    public string CustomerEntityType { get; set; } = null!;
    public string CustomerEntityId { get; set; } = null!;
    public IList<AzurePriceAPIItem> Items { get; set; } = null!;
}

public class AzurePriceAPIItemList : IAzurePriceAPIItemContainer
{
    public IList<AzurePriceAPIItem> Items { get; set; }
    public AzurePriceAPIItemList(IList<AzurePriceAPIItem> items)
    {
        Items = items;
    }
}