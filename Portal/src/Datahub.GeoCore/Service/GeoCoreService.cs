using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Datahub.GeoCore.Service;

internal class GeoCoreService : IGeoCoreService
{
    readonly HttpClient _httpClient;
    readonly GeoCoreConfiguration _config;

    public GeoCoreService(HttpClient httpClient, IOptions<GeoCoreConfiguration> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
    }

    public async Task<GeoCoreResult> PublishDataset(string jsonData)
    {
        var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
        return await PostRequestAsync($"new?source_key={_config.SourceKey}", content);
    }

    public async Task<ShemaValidatorResult> ValidateJson(string data)
    {
        return await Task.FromResult(ShemaValidatorUtil.Validate(data));
    }

    public string GetDatasetUrl(string datasetId, string lang)
    {
        return $"{_config.DatasetBaseUrl}?id={datasetId}&lang={lang}";
    }

    private async Task<GeoCoreResult> PostRequestAsync(string path, HttpContent content)
    {
        try
        {
            // this is to avoid developing on the VPN (test mode should be off in prod)
            if (_config.TestMode)
                return new GeoCoreResult(true, Guid.NewGuid().ToString(), "");

            content.Headers.Add("x-api-key", _config.ApiKey);

            var response = await _httpClient.PostAsync($"{_config.BaseUrl}/{path}", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<GeoCoreApiResult>(jsonResponse, GetSerializationOptions());

            var succeeded = apiResult?.StatusCode == 200;
            var errorMessage = succeeded ? string.Empty : "Request failed!";
            return new GeoCoreResult(succeeded, apiResult?.Body ?? "", errorMessage);
        }
        catch (Exception ex)
        {
            return new GeoCoreResult(false, "", ex.Message);
        }
    }

    static JsonSerializerOptions GetSerializationOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}

/// <summary>
/// Expected json { "statusCode": 200, "body": "b52a1cfd-db6d-4289-afd5-b5851a89456b" }
/// </summary>
class GeoCoreApiResult
{
    public int StatusCode { get; set; }
    public string? Body { get; set; }
}