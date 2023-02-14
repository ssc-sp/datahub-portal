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

    public async Task<double?> GetStorageUsedCapacity(string resourceGroup, string storageAccount)
    {
        var url = GetStorageCapacityRequestUrl(resourceGroup, storageAccount, 2);
        var usageResponse = await _httpClient.GetAsync<StorageUsedResponse>(url, _accessToken, default, _cancellationToken);
        return GetStorageUsedCapacity(usageResponse);
    }

    public async Task<double?> GetResourceGroupMonthlyCost(string resourceGroup)
    {
        var url = GetCostManagementRequestUrl(resourceGroup);
        var request = GetMonthlyCostRequest();
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetResourceGroupCost(response);
    }

    public async Task<List<AzureDailyCost>?> GetResourceGroupMonthlyCostPerDay(string resourceGroup)
    {
        var url = GetCostManagementRequestUrl(resourceGroup);
        var request = GetMonthlyCostPerDayRequest();
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetResourceGroupCostPerDay(response);
    }

    public async Task<double?> GetResourceGroupYesterdayCost(string resourceGroup)
    {
        var url = GetCostManagementRequestUrl(resourceGroup);
        var request = GetYesterdayTotalCostRequest();
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetResourceGroupCost(response);
    }

    public async Task<List<AzureServiceCost>?> GetResourceGroupMonthlyCostByService(string resourceGroup)
    {
        var url = GetCostManagementRequestUrl(resourceGroup);
        var request = GetMonthlyCostByServiceRequest();
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetServiceCosts(response, nameIndex: 2);
    }

    public async Task<List<AzureServiceCost>?> GetResourceGroupYesterdayCostByService(string resourceGroup)
    {
        var url = GetCostManagementRequestUrl(resourceGroup);
        var request = GetYesterdayCostByServiceRequest();
        var response = await _httpClient.PostAsync<CostManagementResponse>(url, _accessToken, GetStringContent(request), _cancellationToken);
        return GetServiceCosts(response, nameIndex: 1);
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

    #region Storage capacity 

    private string GetStorageCapacityRequestUrl(string resourceGroup, string storageAccount, int hours)
    {
        var timespan = GetStorageCapacityTimespan(hours);
        var version = "2019-07-01";
        return $"{AzureManagementUrls.ManagementUrl}/subscriptions/{_configuration.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.Storage/storageAccounts/{storageAccount}/providers/microsoft.Insights/metrics?timespan={timespan}&metricnames=UsedCapacity&aggregation=average&metricNamespace=microsoft.storage%2Fstorageaccounts&validatedimensions=false&api-version={version}";
    }

    static string GetStorageCapacityTimespan(int hours)
    {
        var dt = DateTime.UtcNow;
        var format = "yyyy-MM-ddTHH:mm:00.000Z";
        return $"{dt.AddHours(-hours).ToString(format)}/{dt.ToString(format)}";
    }

    static double? GetStorageUsedCapacity(StorageUsedResponse? response)
    {
        if (response is null)
            return default;

        if (response?.value is null || response.value.Count == 0)
            return default;

        var value = response.value[0];
        if (value.errorCode != "Success" || value.timeseries is null || value.timeseries.Count == 0)
            return default;

        var timeseries = value.timeseries[0];
        if (timeseries is null || timeseries.data.Count == 0)
            return default;

        var data = timeseries.data.OrderByDescending(d => d.timeStamp).FirstOrDefault(d => d.average != 0);

        return data?.average ?? 0.0;
    }

    #endregion

    #region Costing

    private string GetCostManagementRequestUrl(string resourceGroup)
    {
        var version = "2021-10-01";
        return $"{AzureManagementUrls.ManagementUrl}/subscriptions/{_configuration.SubscriptionId}/resourceGroups/{resourceGroup}/providers/Microsoft.CostManagement/query?api-version={version}&$top=5000";
    }

    static CostManagementRequest GetMonthlyCostRequest()
    {
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "Monthly",
                Aggregation = GetCostAggregation()
            }
        };
    }

    static CostManagementRequest GetMonthlyCostPerDayRequest()
    {
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "Daily",
                Aggregation = GetCostAggregation()
            }
        };
    }

    static CostManagementRequest GetYesterdayTotalCostRequest()
    {
        var (from, to) = GetYesterdayPeriod();
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "None",
                Aggregation = GetCostAggregation()
            },
            Timeframe = "Custom",
            TimePeriod = new()
            {
                From = from,
                To = to
            }
        };
    }

    static CostManagementRequest GetMonthlyCostByServiceRequest()
    {
        return new()
        {
            Type = "ActualCost",
            DataSet = new()
            {
                Granularity = "Monthly",
                Aggregation = GetCostAggregation(),
                Grouping = new()
                {
                    new()
                    {
                        Type = "Dimension",
                        Name = "ServiceName"
                    }
                }
            },            
        };
    }

    static CostManagementRequest GetYesterdayCostByServiceRequest()
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
                }
            },
            Timeframe = "Custom",
            TimePeriod = new()
            {
                From = from,
                To = to
            }
        };
    }

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
        .Where(c => c.Cost > 0.01)
        .OrderByDescending(c => c.Cost)
        .ToList();
    }

    static (DateTime from, DateTime to) GetYesterdayPeriod()
    {
        var from = DateTime.UtcNow.AddDays(-1).Date;
        var to = from.AddHours(24).AddSeconds(-1);
        return (from, to);
    }

    static string ParseString(object o) => o?.ToString() ?? string.Empty;

    static double ParseDouble(object o) => double.TryParse(o.ToString(), out double v) ? v : default;

    static DateTime ParseDate(object o)
    {
        var strValue = o.ToString();
        return DateTime.ParseExact(strValue!, "yyyyMMdd", CultureInfo.InvariantCulture).Date;
    }

    #endregion
}
