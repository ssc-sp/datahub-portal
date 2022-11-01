using System.Text.Json.Nodes;

namespace Datahub.Core.Services.AzureCosting;

public class CostManagementRow
{
    public decimal TotalCost { get; init; }
    // ReSharper disable once InconsistentNaming
    public decimal TotalCostUSD { get; init; }
    
    #nullable enable
    public string? TagName { get; init; }
    #nullable disable
    
    public CostManagementRow(JsonArray row)
    {
        TotalCost = (decimal?)row[0] ?? 0;
        TotalCostUSD = (decimal?)row[1] ?? 0;
        TagName = (string)row[3];
    }
}