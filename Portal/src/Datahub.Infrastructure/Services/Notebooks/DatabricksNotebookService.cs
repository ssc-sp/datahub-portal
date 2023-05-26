using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Application.Services.Notebooks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Datahub.Infrastructure.Services.Notebooks;

public class DatabricksNotebookService : IDatabricksNotebookService
{
    private readonly ILogger<DatabricksNotebookService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DatabricksNotebookService(
        ILogger<DatabricksNotebookService> logger, 
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<List<string>> ListAllNotebooksAsync()
    {
        // https://adb-1078013913864941.1.azuredatabricks.net/api/2.0/workspace/list?path=%2FShared

        var token = GetBearerToken();
        if (token == null)
        {
            _logger.LogError("No bearer token found");
            return new List<string>();
        }
        
        
        var httpClient = _httpClientFactory.CreateClient();
        var path = "/";
        var notebookUrl = $"https://adb-1078013913864941.1.azuredatabricks.net/api/2.0/workspace/list?path={path}";
        
        // Get the current token
        
        
        // Add the current user's token to the request
        
        using var response = await httpClient.GetAsync(notebookUrl, HttpCompletionOption.ResponseHeadersRead);
        
        response.EnsureSuccessStatusCode();
        var contentString = await response.Content.ReadAsStringAsync();
        var content = JsonSerializer.Deserialize<JsonObject>(contentString);

        return content["objects"]
            .AsArray()
            .Select(node => node["path"].ToString())
            .ToList();
    }
    
    private string GetBearerToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var authorizationHeader = httpContext?.Request.Headers[HeaderNames.Authorization];
        

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return null; // No bearer token found
        }

        var token = authorizationHeader.Substring("Bearer ".Length);
        return token;

    }
}