using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datahub.Infrastructure.Services.Azure;

public class AzureManagementSession
{
    private readonly IAzureServicePrincipalConfig _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _accessToken;
    private readonly CancellationToken _cancellationToken;

    public AzureManagementSession(IAzureServicePrincipalConfig configuration, HttpClient httpClient, string accessToken, CancellationToken cancellationToken)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _accessToken = accessToken;
        _cancellationToken = cancellationToken;
    }

    public string AccessToken => _accessToken;

    public async Task<double?> GetResourceGroupLastYearCost(string[] resourceGroups)
    {
        var url = GetCostManagementRequestUrl();
        var request = GetYearCostRequest(resourceGroups);
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetResourceGroupCost(response);
    }

    public async Task<List<AzureDailyCost>?> GetResourceGroupMonthlyCostPerDay(string[] resourceGroups)
    {
        var url = GetCostManagementRequestUrl();
        var request = GetMonthlyCostPerDayRequest(resourceGroups);
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetResourceGroupCostPerDay(response);
    }

    public async Task<double?> GetResourceGroupYesterdayCost(string[] resourceGroups)
    {
        var url = GetCostManagementRequestUrl();
        var request = GetYesterdayTotalCostRequest(resourceGroups);
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetResourceGroupCost(response);
    }

    public async Task<List<AzureServiceCost>?> GetResourceGroupYearCostByService(string[] resourceGroups)
    {
        var url = GetCostManagementRequestUrl();
        var request = GetYearCostByServiceRequest(resourceGroups);
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetServiceCosts(response, nameIndex: 1);
    }

    public async Task<List<AzureServiceCost>?> GetResourceGroupYesterdayCostByService(string[] resourceGroups)
    {
        var url = GetCostManagementRequestUrl();
        var request = GetYesterdayCostByServiceRequest(resourceGroups);
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetServiceCosts(response, nameIndex: 1);
    }

    public async Task<double> GetTotalAverageStorageCapacity(params string[] resourceGroups)
    {
        double capacity = 0.0;
        await foreach (var account in ListResourceGroupStorage(resourceGroups))
        {
            var accCapacity = await GetStorageAverageCapacity(account.ResourceGroup, account.StorageAccountName);
            capacity += accCapacity;
        }
        return capacity;
    }

    private async Task<double> GetStorageAverageCapacity(string resourceGroupName, string storageAccountName)
    {
        var requestUrl = GetStorageCapacityRequestUrl(resourceGroupName, storageAccountName);
        var response = await _httpClient.GetAsync<StorageCapacityResponse>(requestUrl, _accessToken, null, _cancellationToken);
        
        if (response is null)
            throw new Exception($"Cannot read Storage Capacity of {resourceGroupName}/{storageAccountName}");

        return response.value[0].timeseries[0].data[0].average;
    }

    record ResourceGroupStorage(string ResourceGroup, string StorageAccountName);

    private async IAsyncEnumerable<ResourceGroupStorage> ListResourceGroupStorage(string[] resourceGroups)
    {
        foreach (var rg in resourceGroups)
        {
            var accountNames = await GetStorageAccountNames(rg);
            foreach (var name in accountNames)
            {
                yield return new(rg, name);
            }
        }
    }

    private async Task<List<string>> GetStorageAccountNames(string resourceGroupName)
    {
        var requestUrl = GetStorageAccountNamesRequestUrl(resourceGroupName);
        var response = await _httpClient.GetAsync<GetStorageAccountsResponse>(requestUrl, _accessToken, null, _cancellationToken);

        var accounts = response?.value;
        if (accounts is null)
            return new();

        return accounts.Select(acc => acc.name).ToList();
    }

    static StringContent GetStringContent(object content)
    {
        var jsonContent = JsonSerializer.Serialize(content, GetJsonSerializerOptions());
        return new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
    }

    static JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new() 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    private string GetCostManagementRequestUrl()
    {
        return $"{AzureManagementUrls.ManagementUrl}/subscriptions/{_configuration.SubscriptionId}/providers/Microsoft.CostManagement/query?api-version=2021-10-01&$top=5000";
    }

    private string GetStorageCapacityRequestUrl(string resourceGroupName, string storageAccountName) 
    {
        return $"{AzureManagementUrls.ManagementUrl}/subscriptions/{_configuration.SubscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Storage/storageAccounts/{storageAccountName}/providers/microsoft.insights/metrics?api-version=2021-05-01&metricnames=UsedCapacity";
    }

    private string GetStorageAccountNamesRequestUrl(string resourceGroupName)
    {
        return $"{AzureManagementUrls.ManagementUrl}/subscriptions/{_configuration.SubscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Storage/storageAccounts?api-version=2021-05-01";
    }

    static CostManagementRequest GetYearCostRequest(string[] resourceGroups)
    {
        var (from, to) = GetLastYearPeriod();
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "None",
                Aggregation = GetCostAggregation(),
                Filter = GetResourceGroupsFilter(resourceGroups)
            },
            Timeframe = "Custom",
            TimePeriod = new()
            {
                From = from,
                To = to
            }
        };
    }

    static CostManagementRequest GetMonthlyCostPerDayRequest(string[] resourceGroups)
    {
        var (from, to) = GetLast30DaysPeriod();
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "Daily",
                Aggregation = GetCostAggregation(),
                Filter = GetResourceGroupsFilter(resourceGroups)
            },
            Timeframe = "Custom",
            TimePeriod = new()
            {
                From = from,
                To = to
            }
        };
    }

    static CostManagementRequest GetYesterdayTotalCostRequest(string[] resourceGroups)
    {
        var (from, to) = GetYesterdayPeriod();
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "None",
                Aggregation = GetCostAggregation(),
                Filter = GetResourceGroupsFilter(resourceGroups)
            },
            Timeframe = "Custom",
            TimePeriod = new()
            {
                From = from,
                To = to
            }
        };
    }

    static CostManagementRequest GetYearCostByServiceRequest(string[] resourceGroups)
    {
        var (from, to) = GetLastYearPeriod();
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "None",
                Aggregation = GetCostAggregation(),
                Grouping = new()
                {
                    new()
                    {
                        Type = "Dimension",
                        Name = "ServiceName"
                    }
                },
                Filter = GetResourceGroupsFilter(resourceGroups)
            },
            Timeframe = "Custom",
            TimePeriod = new()
            {
                From = from,
                To = to
            }
        };
    }

    static CostManagementRequest GetYesterdayCostByServiceRequest(string[] resourceGroups)
    {
        var (from, to) = GetYesterdayPeriod();
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "None",
                Aggregation = GetCostAggregation(),
                Grouping = new()
                {
                    new()
                    {
                        Type = "Dimension",
                        Name = "ServiceName"
                    }
                },
                Filter = GetResourceGroupsFilter(resourceGroups)
            },
            Timeframe = "Custom",
            TimePeriod = new()
            {
                From = from,
                To = to
            }
        };
    }

    static CostManagementRequestFilter GetResourceGroupsFilter(string[] rgs) => new()
    {
        Dimensions = new()
        {
            Name = "ResourceGroupName",
            Operator = "in",
            Values = new(rgs)
        }
    };

    static CostManagementRequestAggregation GetCostAggregation() => new()
    {
        TotalCost = new()
        {
            Name = "Cost",
            Function = "Sum"
        }
    };

    static double? GetResourceGroupCost(CostManagementResponse? response)
    {
        if (response is null ||
            response.Properties is null ||
            response.Properties.Rows is null ||
            response.Properties.Rows.Count == 0 ||
            response.Properties.Rows[0].Count == 0)
            return default;

        return ParseDouble(response.Properties.Rows[0][0]);
    }

    static List<AzureDailyCost>? GetResourceGroupCostPerDay(CostManagementResponse? response)
    {
        return response?.Properties?.Rows?.Select(r => new AzureDailyCost()
        {
            Date = ParseDate(r[1]),
            Cost = ParseDouble(r[0]),
        }).ToList();
    }

    static List<AzureServiceCost>? GetServiceCosts(CostManagementResponse? response, int nameIndex)
    {
        return response?.Properties?.Rows?.Select(r => new AzureServiceCost() 
        { 
            Name = ParseString(r[nameIndex]),
            Cost = ParseDouble(r[0])
        })
        .Where(c => c.Cost > 0.0)
        .OrderByDescending(c => c.Cost)
        .ToList();
    }

    static (DateTime from, DateTime to) GetLastYearPeriod()
    {
        var to = DateTime.Now;
        var from = to.AddYears(-1).AddHours(1);
        return (from, to);
    }

    static (DateTime from, DateTime to) GetYesterdayPeriod()
    {
        var from = DateTime.Now.AddDays(-1).Date;
        var to = from.AddHours(24).AddSeconds(-1);
        return (from, to);
    }

    static (DateTime from, DateTime to) GetLast30DaysPeriod()
    {
        var from = DateTime.Now.AddDays(-31).Date;
        var to = from.AddDays(30);
        return (from, to);
    }

    static string ParseString(object o) => o?.ToString() ?? string.Empty;

    static double ParseDouble(object o) => double.TryParse(o.ToString(), out double v) ? v : default;

    static DateTime ParseDate(object o)
    {
        var strValue = o.ToString();
        return DateTime.ParseExact(strValue!, "yyyyMMdd", CultureInfo.InvariantCulture).Date;
    }
}
