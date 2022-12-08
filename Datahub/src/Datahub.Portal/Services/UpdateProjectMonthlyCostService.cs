using System.Text.Json.Nodes;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Datahub.Portal.Services;

public class UpdateProjectMonthlyCostService
{
    private const string FUNCTION_CONFIG_KEY = "UpdateProjectMonthlyCostFunction";
    private const string FUNCTION_URL_CONFIG_KEY = "FunctionUrl";
    private const string FUNCTION_CODE_CONFIG_KEY = "FunctionCode";
    
    private readonly ILogger<UpdateProjectMonthlyCostService> _logger;
    private readonly IConfiguration _configuration;
    
    public UpdateProjectMonthlyCostService(
        ILogger<UpdateProjectMonthlyCostService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<int> UpdateProjectMonthlyCost()
    {
        var functionSection = _configuration.GetSection(FUNCTION_CONFIG_KEY);
        var functionUrl = functionSection[FUNCTION_URL_CONFIG_KEY];
        var functionCode = functionSection[FUNCTION_CODE_CONFIG_KEY];
        var url = $"{functionUrl}?code={functionCode}";
        // call get request to the function and return the result
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            var json = JsonNode.Parse(result)?["numberOfProjectsUpdated"]?.ToString();
            if (int.TryParse(json, out var numberOfProjectsUpdated))
            {
                return numberOfProjectsUpdated;
            }

            return -1;
        }
        _logger.LogError("Failed to update project monthly cost. Status code: {ResponseStatusCode}", response.StatusCode);
        return -1;
    }
}
        