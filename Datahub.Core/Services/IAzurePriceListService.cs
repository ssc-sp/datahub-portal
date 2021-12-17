using Datahub.Core.Data.StorageCostEstimator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IAzurePriceListService
    {
        Task<SavedStorageCostPriceGrid> GetAzureStoragePriceLists();
        static string GenerateAzurePriceListKey(AccessTierType accessTierType, DataRedundancyType dataRedundancy) => $"{accessTierType}.{dataRedundancy}";
    }
}
