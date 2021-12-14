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
        public Task<Dictionary<string, EstimatorPriceList>> GetAzureStoragePriceLists()
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
            
            var priceLists = new Dictionary<string, EstimatorPriceList>()
            {
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Hot, DataRedundancyType.LRS), hotPrices },
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Hot, DataRedundancyType.ZRS), hotPrices },
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Hot, DataRedundancyType.GRS), hotPrices },
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Cool, DataRedundancyType.LRS), coolPrices },
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Cool, DataRedundancyType.ZRS), coolPrices },
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Cool, DataRedundancyType.GRS), coolPrices },
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Archive, DataRedundancyType.LRS), archivePrices },
                {IAzurePriceListService.GenerateAzurePriceListKey(AccessTierType.Archive, DataRedundancyType.GRS), archivePrices }
            };

            return Task.FromResult(priceLists);
        }

        public Task TestApiResponse()
        {
            throw new NotImplementedException();
        }
    }
}
