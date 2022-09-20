using System;
using System.Globalization;
using System.Text.Json.Nodes;

namespace Datahub.Graph.Functions;

public class CostManagementRow
{
    public double TotalCost { get; init; }
    // ReSharper disable once InconsistentNaming
    public double TotalCostUSD { get; init; }
    public DateTime UsageDate { get; init; }
    public string TagKey { get; init; }
    public string? TagName { get; init; }
    public string ChargeType { get; init; }
    public string ResourceType { get; init; }
    public string ResourceGroup { get; init; }
    public string Currency { get; init; }
    
    public CostManagementRow(JsonArray row)
    {
        TotalCost = (double?)row[0] ?? 0;
        TotalCostUSD = (double?)row[1] ?? 0;
        // ReSharper disable once StringLiteralTypo
        UsageDate = DateTime.ParseExact(row[2].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture );
        TagKey = (string)row[3] ?? string.Empty;
        TagName = (string)row[4];
        ChargeType = (string)row[5] ?? string.Empty;
        ResourceType = (string)row[6] ?? string.Empty;
        ResourceGroup =(string)row[7] ?? string.Empty;
        Currency = (string)row[8] ?? string.Empty;
    }
}