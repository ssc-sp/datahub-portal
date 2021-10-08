using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Datahub.Shared.Services
{
    public class ApiCallService : IApiCallService
    {
        private readonly IHttpClientFactory _httpClient;
        private readonly ILogger _logger;
        private readonly IKeyVaultService _keyVaultService;

        public ApiCallService(ILogger<ApiCallService> logger,
                   IHttpClientFactory clientFactory,
                   IKeyVaultService keyVaultService)
        {
            _logger = logger;
            _httpClient = clientFactory;
            _keyVaultService = keyVaultService;
        }

        public async Task<bool> CallApi(UriBuilder builder, string Url, HttpMethod httpMethod, string secretKey = "DataHub-API-Gateway-Key", Dictionary<string, string> headers = null)
        {
            var gatewayKey = await _keyVaultService.GetSecret(secretKey);
            if (headers == null)
                headers = new Dictionary<string, string>() { { "Ocp-Apim-Subscription-Key", gatewayKey } };

            var request = new HttpRequestMessage(httpMethod, builder.ToString());

            foreach (var kvp in headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }
            var client = _httpClient.CreateClient();
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<string> CallGetApi(UriBuilder builder, string Url, string secretKey = "DataHub-API-Gateway-Key", Dictionary<string, string> headers = null)
        {
            var gatewayKey = await _keyVaultService.GetSecret(secretKey);
            if (headers == null)
                headers = new Dictionary<string, string>() { { "Ocp-Apim-Subscription-Key", gatewayKey } };

            var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());

            foreach (var kvp in headers)
            {
                request.Headers.Add(kvp.Key, kvp.Value);
            }
            var client = _httpClient.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadAsStringAsync();
                return results;
            }
            return string.Empty;
        }

        public string getBlobContainerName()
        {
            return "datahub-nrcan" + ("dev" == getEnvSuffix() ? "-dev" : "");
        }

        public async Task<string> getStorageConnString()
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            envName = (envName != null ? envName.ToLower() : "dev");
            if (envName.Equals("development"))
            {
                envName = "dev";
            }

            var blobKey = await _keyVaultService.GetSecret("DataHub-Blob-Access-Key");
            return "DefaultEndpointsProtocol=https;AccountName=datahubstorage" + getEnvSuffix() + ";AccountKey=" + blobKey + ";EndpointSuffix=core.windows.net";
        }

        public async Task<string> GetProjectConnectionString(string accountName)
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            envName = (envName != null ? envName.ToLower() : "dev");
            if (envName.Equals("development"))
            {
                envName = "dev";
            }
            string key = $"datahub-blob-key-{accountName}";
            var accountKey = await _keyVaultService.GetSecret(key);
            return @$"DefaultEndpointsProtocol=https;AccountName=dh{accountName}{envName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
        }

        private string getEnvSuffix()
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            envName = (envName != null ? envName.ToLower() : "dev");
            if (envName.Equals("development"))
            {
                envName = "dev";
            }

            return envName;
        }
    }
}