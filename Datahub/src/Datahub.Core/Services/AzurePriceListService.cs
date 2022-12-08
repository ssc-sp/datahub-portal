using Datahub.Core.Data.CostEstimator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class AzurePriceListService : IAzurePriceListService
    {
        private static class AzureSkuIds
        {

            // block blob v2, canada east
            /*
            public static readonly string HOT_LRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007C";
            public static readonly string HOT_ZRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007H";
            public static readonly string HOT_GRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007B";
            public static readonly string COOL_LRS_BLOCK_SKU_ID = "DZH318Z0BPH7/0077";
            public static readonly string COOL_ZRS_BLOCK_SKU_ID = "DZH318Z0BPH7/0079";
            public static readonly string COOL_GRS_BLOCK_SKU_ID = "DZH318Z0BPH7/0076";
            public static readonly string ARCHIVE_LRS_BLOCK_SKU_ID = "DZH318Z0BPH7/0074";
            public static readonly string ARCHIVE_GRS_BLOCK_SKU_ID = "DZH318Z0BPH7/0073";
            */

            // block blob v2, canada central
            public static readonly string HOT_LRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007Q";
            public static readonly string HOT_ZRS_BLOCK_SKU_ID = "DZH318Z0BPH7/0086";
            public static readonly string HOT_GRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007N";
            public static readonly string COOL_LRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007G";
            public static readonly string COOL_ZRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007K";
            public static readonly string COOL_GRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007D";
            public static readonly string ARCHIVE_LRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007M";
            public static readonly string ARCHIVE_GRS_BLOCK_SKU_ID = "DZH318Z0BPH7/007L";
            
            // geo replication v2, canada central
            public static readonly string GEO_REPLICATION_SKU_ID = "DZH318Z0BZ26/0021";

            // DSv2 VM series, Canada central
            public static readonly string STANDARD_DS3_V2_SKU_ID = "DZH318Z0BQ4C/0168";
            public static readonly string STANDARD_DS4_V2_SKU_ID = "DZH318Z0BQ4C/0176";
            public static readonly string STANDARD_DS5_V2_SKU_ID = "DZH318Z0BQ4C/011S";

            // Standard all-purpose compute DBU, Canada central
            public static readonly string STANDARD_ALL_PURPOSE_COMPUTE_DBU_SKU_ID = "DZH318Z0BWQJ/004Z";


            public static readonly Dictionary<(AccessTierType, DataRedundancyType), string> StorageSkuMaps = new()
            {
                { (AccessTierType.Hot, DataRedundancyType.LRS), HOT_LRS_BLOCK_SKU_ID },
                { (AccessTierType.Hot, DataRedundancyType.ZRS), HOT_ZRS_BLOCK_SKU_ID },
                { (AccessTierType.Hot, DataRedundancyType.GRS), HOT_GRS_BLOCK_SKU_ID },
                { (AccessTierType.Cool, DataRedundancyType.LRS), COOL_LRS_BLOCK_SKU_ID },
                { (AccessTierType.Cool, DataRedundancyType.ZRS), COOL_ZRS_BLOCK_SKU_ID },
                { (AccessTierType.Cool, DataRedundancyType.GRS), COOL_GRS_BLOCK_SKU_ID },
                { (AccessTierType.Archive, DataRedundancyType.LRS), ARCHIVE_LRS_BLOCK_SKU_ID },
                { (AccessTierType.Archive, DataRedundancyType.GRS), ARCHIVE_GRS_BLOCK_SKU_ID }
            };
        }

        private static class AzureMeterIds
        {
            // block blob v2, canada central
            public static readonly string ALL_OTHER_OPS = "2a83c93a-7ebb-400a-9a49-6d362bb0cf8b";
            public static readonly string INDEX_TAGS = "ed88bed9-d322-4351-bca8-374764661427";

            public static readonly string ARCHIVE_DATA_RETRIEVAL = "ef9f526b-16ae-467d-8554-bc20d98610f8";
            public static readonly string ARCHIVE_DATA_RETRIEVAL_FREE = "b0891789-1fa5-4a38-9084-e4be1cc9774e";
            public static readonly string ARCHIVE_DATA_WRITE = "6f8a99e6-d613-47b3-9cde-d356be2e3d35";
            public static readonly string ARCHIVE_PRIORITY_DATA_RETRIEVAL = "32da5940-b74a-483e-998f-c75244587ef1";
            public static readonly string ARCHIVE_PRIORITY_READ_OPS = "4e8f6b4a-cdce-4e4c-8124-179f479adf95";
            public static readonly string ARCHIVE_READ_OPS = "ad20bc20-bd78-4bf7-9808-6870ec757c2d";
            public static readonly string ARCHIVE_READ_OPS_FREE = "e4c1f073-8d70-58b1-8d04-81bbbcd33f1c";

            public static readonly string ARCHIVE_GRS_BLOB_INVENTORY = "120dd7cc-08a5-5654-a9ba-85f8a4c38c5f";
            public static readonly string ARCHIVE_GRS_DATA_STORED = "aa3373a8-b870-4e99-88d2-bf527e78061c";
            public static readonly string ARCHIVE_GRS_EARLY_DELETE = "a2705334-3415-4553-a8fd-4202340efb9d";
            public static readonly string ARCHIVE_GRS_WRITE_OPS = "01fde9ea-a677-4341-b151-b49b65d3305e";

            public static readonly string ARCHIVE_LRS_BLOB_INVENTORY = "ddce8de3-346e-5019-9d7f-9cc602c8f1f2";
            public static readonly string ARCHIVE_LRS_DATA_STORED = "1e51f089-c313-40a1-867a-3b9b6bebb631";
            public static readonly string ARCHIVE_LRS_DATA_STORED_FREE = "941d48c0-cdc9-52bf-9c08-24597ba0dd0f";
            public static readonly string ARCHIVE_LRS_EARLY_DELETE = "fd406d7d-b614-4b7f-8e8c-55a4e8555f73";
            public static readonly string ARCHIVE_LRS_WRITE_OPS = "4d52c625-ad69-4175-9178-a8cfe72ee6ba";

            public static readonly string COOL_DATA_RETRIEVAL = "2ef71e5b-5da5-4a4d-b492-5277f79b30ea";
            public static readonly string COOL_READ_OPS = "55d6913c-a5af-4e8c-8e9a-855c536f5166";

            public static readonly string COOL_GRS_BLOB_INVENTORY = "1e909e0b-3668-5f4f-869c-68f44759157d";
            public static readonly string COOL_GRS_DATA_STORED = "9438a523-ecc8-4d27-96e0-fb2fb560a3a0";
            public static readonly string COOL_GRS_EARLY_DELETE = "a777433f-8551-4d01-b288-d66f804385f0";
            public static readonly string COOL_GRS_WRITE_OPS = "e2613a8a-62a3-4bb2-9558-c51db8294b0f";

            public static readonly string COOL_LRS_BLOB_INVENTORY = "6f7f10e0-66cb-53f5-b2dd-8e2fc16210eb";
            public static readonly string COOL_LRS_DATA_STORED = "b24b3aa0-f4ff-4546-9bbd-562b193f9924";
            public static readonly string COOL_LRS_EARLY_DELETE = "bec19907-da6d-4167-8c41-3b489ee0e983";
            public static readonly string COOL_LRS_WRITE_OPS = "1b174502-a775-4055-b885-49631396b81a";

            public static readonly string COOL_ZRS_BLOB_INVENTORY = "9816790e-83c2-5894-998d-074bb0b89bd0";
            public static readonly string COOL_ZRS_DATA_RETRIEVAL = "a9edef25-c0f3-4adf-bed6-ac7eb71209a9";
            public static readonly string COOL_ZRS_DATA_STORED = "c1a13166-ba2d-4a77-94f2-02f02d88a95c";
            public static readonly string COOL_ZRS_EARLY_DELETE = "f2d5a7af-0728-4b00-b831-dbdbd91dcf01";
            public static readonly string COOL_ZRS_READ_OPS = "6d1e57f4-0d34-4e22-b8c6-fef128c80b20";
            public static readonly string COOL_ZRS_WRITE_OPS = "92ca4b6f-718b-43f5-9b83-26af4a018415";

            public static readonly string HOT_READ_OPS = "46b7049d-2f0a-4153-96b9-78fea3f83029";

            public static readonly string HOT_GRS_BLOB_INVENTORY = "f75d0c0c-35b9-517d-8e7e-af68db03a316";
            public static readonly string HOT_GRS_DATA_STORED = "88aab413-fad2-42d6-b205-c881ef9c43c1";
            public static readonly string HOT_GRS_WRITE_OPS = "79f04bb5-2872-4b4e-84c5-c6b6006b4503";

            public static readonly string HOT_LRS_BLOB_INVENTORY = "8458e028-8aff-5440-b036-3b97c5948bb5";
            public static readonly string HOT_LRS_DATA_STORED = "627916dd-3eb1-4b1d-bd1c-9f19593f6d25";
            public static readonly string HOT_LRS_WRITE_OPS = "87594d41-d07d-4257-be17-d4d2ee0d0132";

            public static readonly string HOT_ZRS_BLOB_INVENTORY = "01bbcf8a-6a19-57db-909b-94bf350fe1f4";
            public static readonly string HOT_ZRS_DATA_STORED = "9651b232-5889-4d7f-8fee-7cfcc739bda0";
            public static readonly string HOT_ZRS_READ_OPS = "9324aaa8-417d-46b4-bbb7-d2a20d511198";
            public static readonly string HOT_ZRS_WRITE_OPS = "e3de7e17-ee34-40aa-96ad-b924f71cd771";

            public static readonly string GRS_INDEX_TAGS = "af8ca23b-e66e-4153-8e11-b27e8746a3ed";
            public static readonly string GRS_LIST_CREATE_CONTAINER_OPS = "4c5a580b-d60c-4913-a60b-62e6ce7d1cff";
            public static readonly string LRS_LIST_CREATE_CONTAINER_OPS = "87594d41-d07d-4257-be17-d4d2ee0d0132";
            public static readonly string ZRS_LIST_CREATE_CONTAINER_OPS = "e3de7e17-ee34-40aa-96ad-b924f71cd771";

            public static readonly string GEO_REPLICATION_V2_DATA_TRANSFER = "ea8e044b-900e-4137-b26f-7e41eecef1b6";


            public static readonly string STANDARD_DS3_V2_METER_ID = "121219c8-4963-475a-b861-2cfe6667b9c2";
            public static readonly string STANDARD_DS4_V2_METER_ID = "72cbc933-fe63-4de0-a60d-f37c66dc7d4f";
            public static readonly string STANDARD_DS5_V2_METER_ID = "60596994-ce9c-470c-a8ee-853b694a3037";

            public static readonly string STANDARD_ALL_PURPOSE_COMPUTE_DBU_METER_ID = "1c96d414-32bd-4360-acbf-48f233d03005";


            public static readonly List<string> DataStored = new()
            {
                HOT_LRS_DATA_STORED,
                HOT_ZRS_DATA_STORED,
                HOT_GRS_DATA_STORED,
                COOL_LRS_DATA_STORED,
                COOL_ZRS_DATA_STORED,
                COOL_GRS_DATA_STORED,
                ARCHIVE_GRS_DATA_STORED,
                ARCHIVE_LRS_DATA_STORED
            };

            public static readonly List<string> WriteOperations = new()
            {
                HOT_LRS_WRITE_OPS,
                HOT_ZRS_WRITE_OPS,
                HOT_GRS_WRITE_OPS,
                COOL_LRS_WRITE_OPS,
                COOL_ZRS_WRITE_OPS,
                COOL_GRS_WRITE_OPS,
                ARCHIVE_GRS_WRITE_OPS,
                ARCHIVE_LRS_WRITE_OPS
            };
            
            public static readonly List<string> ListAndCreateContainer = new()
            {
                GRS_LIST_CREATE_CONTAINER_OPS,
                LRS_LIST_CREATE_CONTAINER_OPS,
                ZRS_LIST_CREATE_CONTAINER_OPS
            };

            public static readonly List<string> ReadOperations = new()
            {
                ARCHIVE_READ_OPS,
                COOL_READ_OPS,
                HOT_READ_OPS,
                HOT_ZRS_READ_OPS,
                COOL_ZRS_READ_OPS
            };

            public static readonly List<string> ArchivePriorityRead = new() { ARCHIVE_PRIORITY_READ_OPS };

            public static readonly List<string> DataRetrieval = new()
            {
                ARCHIVE_DATA_RETRIEVAL,
                COOL_DATA_RETRIEVAL,
                COOL_ZRS_DATA_RETRIEVAL
            };

            public static readonly List<string> DataWrite = new() { ARCHIVE_DATA_WRITE };

            public static readonly List<string> OtherOperations = new() { ALL_OTHER_OPS };

            public static readonly List<string> GeoReplication = new() { GEO_REPLICATION_V2_DATA_TRANSFER };

        }

        private readonly Dictionary<string, int> MeasurementUnitConversions = new()
        {
            { "10K", 10000 },
            { "10K/Month", 10000 },
            { "1 GB/Month", 1 },
            { "1 GB", 1 },
            { "1M", 1000000 },
            { "1 Hour", 1 }
        };

        private static readonly string API_BASE_URL = "https://prices.azure.com/api/retail/prices";
        private static readonly string SAVED_STORAGE_PRICE_LIST_ID = "StoragePriceList";
        private static readonly string SAVED_COMPUTE_PRICE_LIST_ID = "ComputePriceList";

        private static readonly UnitPrice zeroPrice = new(0.0M, 1);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMiscStorageService _miscStorageService;

        public AzurePriceListService(
            IHttpClientFactory httpClientFactory, 
            IMiscStorageService miscStorageService
            )
        {
            _httpClientFactory = httpClientFactory;
            _miscStorageService = miscStorageService;
        }

        private static string BuildApiUrl(string skuId) => $"{API_BASE_URL}?currencyCode='CAD'&$filter=skuId eq '{skuId}'";
        private static string BuildMultiSkuApiUrl(IEnumerable<string> skuIds)
        {
            StringBuilder sb = new();

            sb.Append($"{API_BASE_URL}?currencyCode='CAD'&$filter=");
            foreach (var it in skuIds.Select((sku,index) => (sku, index)))
            {
                if (it.index > 0)
                {
                    sb.Append(" or ");
                }
                sb.Append($"skuId eq '{it.sku}'");
            }

            return sb.ToString();
        }

        private UnitPrice GetUnitPriceFromApiResult(IAzurePriceAPIItemContainer apiResult, IEnumerable<string> meterIds)
        {
            var correspondingItem = apiResult.Items
                .Where(i => meterIds.Contains(i.MeterId))
                .OrderBy(i => i.TierMinimumUnits) // default to lowest tier; maybe add proper tier handling later
                .FirstOrDefault();

            if (correspondingItem == null)
            {
                return zeroPrice;
            }
            else
            {
                return new(correspondingItem.RetailPrice, MeasurementUnitConversions[correspondingItem.UnitOfMeasure]);
            }
        }

        private UnitPrice GetUnitPriceFromApiResult(IAzurePriceAPIItemContainer apiResult, string meterId) =>
            GetUnitPriceFromApiResult(apiResult, new List<string>() { meterId });

        private decimal GetDecimalPriceFromApiResult(IAzurePriceAPIItemContainer apiResult, string meterId) =>
            GetUnitPriceFromApiResult(apiResult, meterId).BasePrice;

        private async Task<AzurePriceAPIResult> GetAzureApiResult(string url)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            using var response = await httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AzurePriceAPIResult>(content);
            return result;
        }

        private static AzurePriceAPIItemList GetPricesForSku(AzurePriceAPIResult result, string skuId)
        {
            return new(result.Items.Where(i => i.SkuId == skuId).ToList());
        }

        private async Task<SavedStorageCostPriceGrid> FetchSavedStoragePriceGrid() => await _miscStorageService.GetObject<SavedStorageCostPriceGrid>(SAVED_STORAGE_PRICE_LIST_ID);
        private async Task<ComputeCostEstimatorPrices> FetchSavedComputePriceList() => await _miscStorageService.GetObject<ComputeCostEstimatorPrices>(SAVED_COMPUTE_PRICE_LIST_ID);

        private async Task SaveStoragePriceGrid(SavedStorageCostPriceGrid priceGrid) => await _miscStorageService.SaveObject(priceGrid, SAVED_STORAGE_PRICE_LIST_ID);
        private async Task SaveComputePriceList(ComputeCostEstimatorPrices priceGrid) => await _miscStorageService.SaveObject(priceGrid, SAVED_COMPUTE_PRICE_LIST_ID);

        private async Task<Dictionary<string, StorageCostEstimatorPriceList>> GenerateStoragePriceListFromApi()
        {
            var geoRepUrl = BuildApiUrl(AzureSkuIds.GEO_REPLICATION_SKU_ID);
            var geoRepResult = await GetAzureApiResult(geoRepUrl);
            var geoRepPrice = GetUnitPriceFromApiResult(geoRepResult, AzureMeterIds.GeoReplication);

            var priceListUrl = BuildMultiSkuApiUrl(AzureSkuIds.StorageSkuMaps.Values);
            var priceResult = await GetAzureApiResult(priceListUrl);

            var finalResult = AzureSkuIds.StorageSkuMaps
                .Select(kvp =>
                {
                    var rawSkuPrices = GetPricesForSku(priceResult, kvp.Value);
                    var skuPriceList = new StorageCostEstimatorPriceList()
                    {
                        ArchiveHPRead = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.ArchivePriorityRead),
                        Capacity = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.DataStored),
                        DataRetrieval = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.DataRetrieval),
                        DataWrite = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.DataWrite),
                        GeoReplication = (kvp.Key.Item2 == DataRedundancyType.GRS) ? geoRepPrice : zeroPrice,
                        ListCreateOperations = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.ListAndCreateContainer),
                        OtherOperations = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.OtherOperations),
                        ReadOperations = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.ReadOperations),
                        WriteOperations = GetUnitPriceFromApiResult(rawSkuPrices, AzureMeterIds.WriteOperations)
                    };
                    return (IAzurePriceListService.GenerateAzureStoragePriceListKey(kvp.Key.Item1, kvp.Key.Item2), skuPriceList);
                })
                .ToDictionary(tuple => tuple.Item1, tuple => tuple.skuPriceList);

            return finalResult;
        }

        public async Task<SavedStorageCostPriceGrid> GetAzureStoragePriceLists()
        {
            var yesterday = DateTime.UtcNow - TimeSpan.FromDays(1);

            var priceGrid = await FetchSavedStoragePriceGrid();
            
            if (priceGrid == null || priceGrid.LastUpdatedUtc <= yesterday)
            {
                var apiPriceList = await GenerateStoragePriceListFromApi();
                var utcNow = DateTime.UtcNow;

                priceGrid = new SavedStorageCostPriceGrid()
                {
                    LastUpdatedUtc = utcNow,
                    PriceLists = apiPriceList
                };

                await SaveStoragePriceGrid(priceGrid);
            }

            return priceGrid;
        }

        private async Task<ComputeCostEstimatorPrices> GetAzureDatabricksComputePriceListFromApi()
        {
            var computeSkus = new List<string>()
            {
                AzureSkuIds.STANDARD_DS3_V2_SKU_ID,
                AzureSkuIds.STANDARD_DS4_V2_SKU_ID,
                AzureSkuIds.STANDARD_DS5_V2_SKU_ID,
                AzureSkuIds.STANDARD_ALL_PURPOSE_COMPUTE_DBU_SKU_ID
            };

            var apiUrl = BuildMultiSkuApiUrl(computeSkus);
            var priceResult = await GetAzureApiResult(apiUrl);

            var priceGrid = new ComputeCostEstimatorPrices()
            {
                LastUpdatedUtc = DateTime.UtcNow,
                Ds3VmPrice = GetDecimalPriceFromApiResult(priceResult, AzureMeterIds.STANDARD_DS3_V2_METER_ID),
                Ds4VmPrice = GetDecimalPriceFromApiResult(priceResult, AzureMeterIds.STANDARD_DS4_V2_METER_ID),
                Ds5VmPrice = GetDecimalPriceFromApiResult(priceResult, AzureMeterIds.STANDARD_DS5_V2_METER_ID),
                DbuPrice = GetDecimalPriceFromApiResult(priceResult, AzureMeterIds.STANDARD_ALL_PURPOSE_COMPUTE_DBU_METER_ID)
            };

            return priceGrid;
        }

        public async Task<ComputeCostEstimatorPrices> GetAzureComputeCostPrices()
        {
            var yesterday = DateTime.UtcNow - TimeSpan.FromDays(1);

            var priceGrid = await FetchSavedComputePriceList();

            if (priceGrid == null || priceGrid.LastUpdatedUtc <= yesterday)
            {
                priceGrid = await GetAzureDatabricksComputePriceListFromApi();

                await SaveComputePriceList(priceGrid);
            }

            return priceGrid;
        }
    }
}
