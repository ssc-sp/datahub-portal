using Datahub.Core.Data.CostEstimator;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public interface IAzurePriceListService
    {
        Task<SavedStorageCostPriceGrid> GetAzureStoragePriceLists();
        static string GenerateAzureStoragePriceListKey(AccessTierType accessTierType, DataRedundancyType dataRedundancy) => $"{accessTierType}.{dataRedundancy}";
        Task<ComputeCostEstimatorPrices> GetAzureComputeCostPrices();
    }
}
