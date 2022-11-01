using System;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Datahub.Core.Services.AzureCosting;
public class CostManagementRequestBody
{
    public string type { get; } = "ActualCost";
    public DataSet dataSet { get; set; } = new();
    public string timeframe { get; } = "Custom";
    public TimePeriod timePeriod { get; set; }
}
public class Aggregation
{
    public TotalCost totalCost { get; } = new();
    public TotalCostUSD totalCostUSD { get; } = new();
}

public class DataSet
{
    public string granularity { get; } = "None";
    public Aggregation aggregation { get;  } = new();
    public List<Sorting> sorting { get; } = new() { new Sorting() };

    public List<Grouping> grouping { get; } = new()
    {
        new Grouping()
        {
            type = "TagKey",
            name = "datahub_project"
        },
        new Grouping()
        {
            type = "Dimension",
            name = "ChargeType"
        },
        new Grouping()
        {
            type = "Dimension",
            name = "ResourceType"
        },
        new Grouping()
        {
            type = "Dimension",
            name = "ResourceGroupName"
        },
    };
}

public class Grouping
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string type { get; set; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string name { get; set; }
}


public class Sorting
{
    public string direction { get; set; } = "ascending";
    public string name { get; set; } = "UsageDate";
}

public class TimePeriod
{
    public DateTime from { get; set; }
    public DateTime to { get; set; }
}

public class TotalCost
{
    public string name { get; } = "Cost";
    public string function { get; } = "Sum";
}

public class TotalCostUSD
{
    public string name { get; } = "CostUSD";
    public string function { get; } = "Sum";
}

