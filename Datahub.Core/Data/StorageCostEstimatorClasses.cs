using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


}
