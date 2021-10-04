using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NRCan.Datahub.Shared.Data;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public class CKANService : ICKANService
    {
        readonly HttpClient _httpClient;
        readonly IOptions<CKANConfiguration> _ckanConfiguration;

        public CKANService(HttpClient httpClient, IOptions<CKANConfiguration> ckanConfiguration)
        {
            _httpClient = httpClient;
            _ckanConfiguration = ckanConfiguration;
        }

        public async Task<CKANApiResult> CreatePackage(IDictionary<string, object> packageData)
        {
            // generate json from package
            var jsonData = JsonConvert.SerializeObject(packageData);

            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var baseUrl = _ckanConfiguration.Value.BaseUrl;
            var apiKey = _ckanConfiguration.Value.ApiKey;

            content.Headers.Add("X-CKAN-API-Key", apiKey);

            try
            {
                var response = await _httpClient.PostAsync($"{baseUrl}/package_create", content);

                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var ckanResult = JsonConvert.DeserializeObject<CKANResult>(jsonResponse);

                return new CKANApiResult(ckanResult.Success, ckanResult.Error?.Message);
            }
            catch (HttpRequestException ex)
            {
                return new CKANApiResult(false, ex.Message);
            }
        }
    }

    class CKANResult
    {
        public string Help { get; set; }
        public bool Success { get; set; }
        public object Result {  get; set; }
        public CKANError Error { get; set; }
    }

    class CKANError
    {
        public string Message { get; set; }
        public string __type { get; set; }
    }
}
