namespace Datahub.Infrastructure.Services.Azure;

public class AzureServiceCost
{
    public required string Name { get; set; }
    public required double Cost { get; set; }
    
    public DateTime Date { get; set; }
}

public class AzureDailyCost
{
    public DateTime Date{ get; set; }
    public required double Cost { get; set; }
}


#region Cost management  request

class CostManagementRequest
{
    public required string Type { get; set; }
    public required CostManagementRequestDataSet DataSet { get; set; }
    public string? Timeframe { get; set; }
    public CostManagementRequestTimePeriod? TimePeriod { get; set; }
}

class CostManagementRequestDataSet
{
    public required string Granularity { get; set; }
    public required CostManagementRequestAggregation Aggregation { get; set; }
    public List<CostManagementRequestSorting>? Sorting { get; set; }
    public List<CostManagementRequestGrouping>? Grouping { get; set; }
    public CostManagementRequestFilter? Filter { get; set; }
}

class CostManagementRequestTimePeriod
{
    public required DateTime From { get; set; }
    public required DateTime To { get; set; }
}

class CostManagementRequestAggregation
{
    public required CostManagementRequestTotalCost TotalCost { get; set; }
    public CostManagementRequestTotalCost? TotalCostUSD { get; set; }
}

class CostManagementRequestTotalCost
{
    public required string Name { get; set; }
    public required string Function { get; set; }
}

class CostManagementRequestSorting
{
    public required string Direction { get; set; }
    public required string Name { get; set; }
}

class CostManagementRequestGrouping
{
    public required string Type { get; set; }
    public required string Name { get; set; }
}

public class CostManagementRequestFilter
{
    public required CostManagementRequestDimensions Dimensions { get; set; }
}

public class CostManagementRequestDimensions
{
    public required string Name { get; set; }
    public required string Operator { get; set; }
    public List<string> Values { get; set; } = new();
}

#endregion

#region Cost management response

public class CostManagementResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public object? Location { get; set; }
    public object? Sku { get; set; }
    public object? ETag { get; set; }
    public CostManagementResponseProperties? Properties { get; set; }
}

public class CostManagementResponseColumn
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}

public class CostManagementResponseProperties
{
    public object? NextLink { get; set; }
    public required List<CostManagementResponseColumn> Columns { get; set; }
    public required List<List<object>> Rows { get; set; }
}

#endregion 
