namespace Datahub.Core.Data.CostEstimators;

public class StorageCostEstimatorResult
{
    public EstimatorResultLine WriteOperations { get; set; } = null!;
    public EstimatorResultLine ListCreateOperations { get; set; } = null!;
    public EstimatorResultLine ReadOperations { get; set; } = null!;
    public EstimatorResultLine OtherOperations { get; set; } = null!;
    public EstimatorResultLine DataRetrieval { get; set; } = null!;
    public EstimatorResultLine DataWrite { get; set; } = null!;
    public EstimatorResultLine GeoReplication { get; set; } = null!;

    private decimal Cost(EstimatorResultLine l) => l?.Cost ?? 0.0000M;
    public bool HasValues => (WriteOperations ?? ListCreateOperations ?? ReadOperations ?? OtherOperations ?? DataRetrieval ?? DataWrite ?? GeoReplication) != null;
    public decimal TotalCost => Cost(WriteOperations) + Cost(ListCreateOperations) + Cost(ReadOperations) +
                                Cost(OtherOperations) + Cost(DataRetrieval) + Cost(DataWrite) + Cost(GeoReplication);
}
public class StorageCostEstimatorPriceList
{
    public UnitPrice Capacity { get; set; } = null!;
    public UnitPrice WriteOperations { get; set; } = null!;
    public UnitPrice ListCreateOperations { get; set; } = null!;
    public UnitPrice ReadOperations { get; set; } = null!;
    public UnitPrice ArchiveHPRead { get; set; } = null!;
    public UnitPrice DataRetrieval { get; set; } = null!;
    public UnitPrice DataWrite { get; set; } = null!;
    public UnitPrice OtherOperations { get; set; } = null!;
    public UnitPrice GeoReplication { get; set; } = null!;
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

public class SavedStorageCostPriceGrid
{
    public DateTime LastUpdatedUtc { get; set; }

    public Dictionary<string, StorageCostEstimatorPriceList> PriceLists { get; set; } = new();
}
