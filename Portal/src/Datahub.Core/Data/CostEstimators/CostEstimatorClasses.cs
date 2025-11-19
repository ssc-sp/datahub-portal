namespace Datahub.Core.Data.CostEstimators;

/// <summary>
/// Represents a single line in a cost estimator result with an integer number of operations.
/// </summary>
public record class EstimatorResultLine(int NumberOfOperations, decimal Cost);

/// <summary>
/// Represents a single line in a cost estimator result with a decimal number of operations (high granularity scenarios).
/// </summary>
public record class EstimatorResultLineDecimal(decimal NumberOfOperations, decimal Cost);

/// <summary>
/// Represents a unit price composed of a base price and the number of units that price covers.
/// </summary>
public record class UnitPrice(decimal BasePrice, int Units);

/// <summary>
/// A single retail price item returned by the Azure Retail Prices API.
/// Source: Azure Retail Prices API (GET https://prices.azure.com/api/retail/prices) and documentation
/// https://learn.microsoft.com/azure/cost-management-billing/costs/quick-access-retail-prices (overview) and
/// https://learn.microsoft.com/rest/api/cost-management/retail-prices/azure-retail-prices (REST reference)
/// </summary>
/// <remarks>
/// Properties map1:1 to the JSON fields in the response. Optional fields are marked nullable.
/// The API is paged; see <see cref="AzurePriceAPIResult.NextPageLink"/> for pagination.
/// </remarks>
public class AzurePriceAPIItem
{
    /// <summary>Gets or sets the unique identifier for the meter.</summary>
    public string MeterId { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the meter (e.g., CPU Hours, Data Stored).</summary>
    public string MeterName { get; set; } = string.Empty;

    /// <summary>Gets or sets the unique identifier for the product.</summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the product (e.g., Virtual Machines).</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Gets or sets the SKU identifier.</summary>
    public string SkuId { get; set; } = string.Empty;

    /// <summary>Gets or sets the SKU name.</summary>
    public string SkuName { get; set; } = string.Empty;

    /// <summary>Gets or sets the service identifier.</summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>Gets or sets the service name (e.g., Azure Compute).</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional broader service family grouping (e.g., Compute, Storage).</summary>
    public string? ServiceFamily { get; set; }

    /// <summary>Gets or sets the unit of measure for price (e.g.,1 Hour,10 GB).</summary>
    public string UnitOfMeasure { get; set; } = string.Empty;

    /// <summary>Gets or sets the unit price in the specified currency. May be zero when RetailPrice is provided instead.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Gets or sets the retail price before any negotiated discounts. Often equals <see cref="UnitPrice"/>.</summary>
    public decimal RetailPrice { get; set; }

    /// <summary>Gets or sets the optional ARM (Azure Resource Manager) SKU name.</summary>
    public string? ArmSkuName { get; set; }

    /// <summary>Gets or sets the optional ARM region name (e.g., canadacentral).</summary>
    public string? ArmRegionName { get; set; }

    /// <summary>Gets or sets the optional display location (e.g., Canada Central).</summary>
    public string? Location { get; set; }

    /// <summary>Gets or sets the currency code (e.g., CAD, USD) for prices.</summary>
    public string CurrencyCode { get; set; } = string.Empty;

    /// <summary>Gets or sets the minimum units at this tier (for tiered pricing scenarios).</summary>
    public decimal TierMinimumUnits { get; set; }

    /// <summary>Gets or sets the date when this price became effective.</summary>
    public DateTime EffectiveStartDate { get; set; }

    /// <summary>Gets or sets a value indicating whether this meter is in the primary region for the service.</summary>
    public bool IsPrimaryMeterRegion { get; set; }

    /// <summary>Gets or sets the type of item (commonly "Retail" or similar categorization).</summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Common container interface implemented by price API result sets.
/// </summary>
public interface IAzurePriceAPIItemContainer
{
    /// <summary>Gets or sets the list of retail price items.</summary>
    IList<AzurePriceAPIItem> Items { get; set; }
}

/// <summary>
/// Represents a page of results returned by the Azure Retail Prices API.
/// </summary>
public class AzurePriceAPIResult : IAzurePriceAPIItemContainer
{
    /// <summary>Gets or sets the billing currency for prices in this result set (e.g., CAD).</summary>
    public string BillingCurrency { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of items returned in this page.</summary>
    public int Count { get; set; }

    /// <summary>Gets or sets the link to the next page of results; null when no further pages exist.</summary>
    public string? NextPageLink { get; set; }

    /// <summary>Gets or sets the customer entity type associated with the query.</summary>
    public string CustomerEntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the customer entity identifier.</summary>
    public string CustomerEntityId { get; set; } = string.Empty;

    /// <summary>Gets or sets the retail price items contained in this page.</summary>
    public IList<AzurePriceAPIItem> Items { get; set; } = new List<AzurePriceAPIItem>();
}

/// <summary>
/// Simple wrapper around a list of <see cref="AzurePriceAPIItem"/> when pagination metadata is not required.
/// </summary>
public class AzurePriceAPIItemList : IAzurePriceAPIItemContainer
{
    /// <inheritdoc />
    public IList<AzurePriceAPIItem> Items { get; set; }

    public AzurePriceAPIItemList(IList<AzurePriceAPIItem> items)
    {
        Items = items;
    }
}