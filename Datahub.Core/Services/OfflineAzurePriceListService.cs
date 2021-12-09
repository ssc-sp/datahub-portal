using Datahub.Core.Data.StorageCostEstimator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class OfflineAzurePriceListService : IAzurePriceListService
    {
        public Task<Dictionary<(AccessTierType, DataRedundancyType), EstimatorPriceList>> GetAzureStoragePriceLists()
        {
            EstimatorPriceList hotPrices = new()
            {
                Capacity = new(20.48M, 1000),
                WriteOperations = new(0.055M, 10000),
                ListCreateOperations = new(0.055M, 10000),
                ReadOperations = new(0.004M, 10000),
                ArchiveHPRead = new(0.0M, 10000),
                DataRetrieval = new(0.0M, 1000),
                DataWrite = new(0.0M, 1000),
                OtherOperations = new(0.004M, 10000),
                GeoReplication = new(0.022M, 1000)
            };

            EstimatorPriceList coolPrices = new()
            {
                Capacity = new(11.26M, 1000),
                WriteOperations = new(0.1M, 10000),
                ListCreateOperations = new(0.055M, 10000),
                ReadOperations = new(0.01M, 10000),
                ArchiveHPRead = new(0.0M, 10000),
                DataRetrieval = new(10.0M, 1000),
                DataWrite = new(0.0M, 1000),
                OtherOperations = new(0.004M, 10000),
                GeoReplication = new(0.022M, 1000)
            };

            EstimatorPriceList archivePrices = new()
            {
                Capacity = new(1.84M, 1000),
                WriteOperations = new(0.11M, 10000),
                ListCreateOperations = new(0.055M, 10000),
                ReadOperations = new(5.5M, 10000),
                ArchiveHPRead = new(60.0M, 10000),
                DataRetrieval = new(22.0M, 1000),
                DataWrite = new(0.0M, 1000),
                OtherOperations = new(0.004M, 10000),
                GeoReplication = new(0.022M, 1000)
            };

            var priceLists = new Dictionary<(AccessTierType, DataRedundancyType), EstimatorPriceList>()
            {
                {(AccessTierType.Hot, DataRedundancyType.LRS), hotPrices },
                {(AccessTierType.Hot, DataRedundancyType.ZRS), hotPrices },
                {(AccessTierType.Hot, DataRedundancyType.GRS), hotPrices },
                {(AccessTierType.Cool, DataRedundancyType.LRS), coolPrices },
                {(AccessTierType.Cool, DataRedundancyType.ZRS), coolPrices },
                {(AccessTierType.Cool, DataRedundancyType.GRS), coolPrices },
                {(AccessTierType.Archive, DataRedundancyType.LRS), archivePrices },
                {(AccessTierType.Archive, DataRedundancyType.GRS), archivePrices }
            };

            return Task.FromResult(priceLists);
        }

        public Task TestApiResponse()
        {
            throw new NotImplementedException();
        }
    }
}
