using Azure;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.Services.Api;

public class ApplicationInsightsService
{
    private readonly LogsQueryClient _client;
    private readonly ILogger<ApplicationInsightsService> _logger;
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationInsightsService"/> class.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="configuration"></param>
    public ApplicationInsightsService(ILogger logger, IConfiguration configuration)
    {
        _logger = (ILogger<ApplicationInsightsService>)logger;
        _config = configuration;
        _client = new LogsQueryClient(new DefaultAzureCredential());
    }

    public async Task<List<InfrastructureHealthResult>> GetHealthResultsAsync(string workspaceId)
    {
        string query = @"
            customEvents
            | where name == 'InfrastructureHealthCheck'
            | summarize count() by tostring(customDimesions.resourceType), tostring(customDimesions.status) ";
        Response<LogsQueryResult> response = await _client.QueryWorkspaceAsync(
            workspaceId,
            query,
            new QueryTimeRange(TimeSpan.FromDays(1)));
        List<InfrastructureHealthResult> results = response.Value.Table.Rows.Select(x => new InfrastructureHealthResult
        {
            ResourceType = x["tostring_customDimesions_resourceType"].ToString(),
            Status = x["tostring_customDimesions_status"].ToString(),
            Count = Convert.ToInt32(x["count_"].ToString())
        }).ToList();
        return results;
    }
}