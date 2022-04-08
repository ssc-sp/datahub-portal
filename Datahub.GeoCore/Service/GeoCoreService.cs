using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Datahub.GeoCore.Service
{
    internal class GeoCoreService : IGeoCoreService
    {
        readonly HttpClient _httpClient;
        readonly GeoCoreConfiguration _config;

        public GeoCoreService(HttpClient httpClient, IOptions<GeoCoreConfiguration> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }
        
        public async Task<GeoCoreResult> CreatePackage(string jsonData)
        {
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            return await PostRequestAsync("new", content);
        }

        private async Task<GeoCoreResult> PostRequestAsync(string action, HttpContent content)
        {
            try
            {
                // this is to avoid developing on the VPN (test mode should be off in prod)
                if (_config.TestMode)
                    return new GeoCoreResult(true, "");

                var baseUrl = _config.BaseUrl;
                var apiKey = _config.ApiKey;

                content.Headers.Add("x_api_key", apiKey);

                var response = await _httpClient.PostAsync($"{baseUrl}/{action}", content);
                //response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<GeoCoreApiResult>(jsonResponse, GetSerializationOptions());

                var succeeded = apiResult?.Success == true;
                var errorMessage = succeeded ? string.Empty : "Request failed (todo)";
                return new GeoCoreResult(succeeded, errorMessage);
            }
            catch (Exception ex)
            {
                return new GeoCoreResult(false, ex.Message);
            }
        }

        static JsonSerializerOptions GetSerializationOptions() => new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    class GeoCoreApiResult
    {
        public bool Success { get; set; }
    }
}
