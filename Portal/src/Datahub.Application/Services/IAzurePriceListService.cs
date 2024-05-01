using Datahub.Core.Data.CostEstimators;

namespace Datahub.Application.Services;

public interface IAzurePriceListService
{
    Task<SavedStorageCostPriceGrid> GetAzureStoragePriceLists();
    static string GenerateAzureStoragePriceListKey(AccessTierType accessTierType, DataRedundancyType dataRedundancy) => $"{accessTierType}.{dataRedundancy}";
    Task<ComputeCostEstimatorPrices> GetAzureComputeCostPrices();
}