using System;
using System.Collections.Generic;

namespace Datahub.Core.Data.CostEstimator
{
    public class StorageCostEstimatorResult
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
    public class StorageCostEstimatorPriceList
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

    public class SavedStorageCostPriceGrid
    {
        public DateTime LastUpdatedUtc { get; set; }
        
        public Dictionary<string, StorageCostEstimatorPriceList> PriceLists { get; set; }
    }

}
