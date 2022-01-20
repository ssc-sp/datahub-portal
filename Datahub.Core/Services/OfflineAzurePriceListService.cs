using Datahub.Core.Data.CostEstimator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class OfflineAzurePriceListService : IAzurePriceListService
    {
        public async Task<ComputeCostEstimatorPrices> GetAzureComputeCostPrices()
        {
            return await Task.FromResult(new ComputeCostEstimatorPrices()
            {
                LastUpdatedUtc = DateTime.UtcNow,
                DbuPrice = 0.52M,
                Ds3VmPrice = 0.36M,
                Ds4VmPrice = 0.72M,
                Ds5VmPrice = 1.43M
            });
        }

        public Task<SavedStorageCostPriceGrid> GetAzureStoragePriceLists()
        {
            StorageCostEstimatorPriceList hotPrices = new()
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

            StorageCostEstimatorPriceList coolPrices = new()
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

            StorageCostEstimatorPriceList archivePrices = new()
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
            
            var priceLists = new Dictionary<string, StorageCostEstimatorPriceList>()
            {
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Hot, DataRedundancyType.LRS), hotPrices },
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Hot, DataRedundancyType.ZRS), hotPrices },
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Hot, DataRedundancyType.GRS), hotPrices },
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Cool, DataRedundancyType.LRS), coolPrices },
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Cool, DataRedundancyType.ZRS), coolPrices },
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Cool, DataRedundancyType.GRS), coolPrices },
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Archive, DataRedundancyType.LRS), archivePrices },
                {IAzurePriceListService.GenerateAzureStoragePriceListKey(AccessTierType.Archive, DataRedundancyType.GRS), archivePrices }
            };

            return Task.FromResult(new SavedStorageCostPriceGrid()
            {
                LastUpdatedUtc = DateTime.UtcNow,
                PriceLists = priceLists
            });
        }
    }
}
