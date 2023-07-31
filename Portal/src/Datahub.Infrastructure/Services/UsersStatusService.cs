using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Datahub.Infrastructure.Services
{
    public partial class UsersStatusService : IUsersStatusService
    {
        private readonly ILogger<UsersStatusService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
        
        public UsersStatusService(ILogger<UsersStatusService> logger, IHttpClientFactory httpClientFactory,
            DatahubPortalConfiguration datahubPortalConfiguration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _datahubPortalConfiguration = datahubPortalConfiguration;
        }
        
        public async Task<Dictionary<string, List<string>>> GetUsersStatus()
        {
            var url = _datahubPortalConfiguration.DatahubGraphUsersStatusFunctionUrl;
            
            var numberOfRetries = 0;
            const int maxNumberOfRetries = 5;
            string resultString;
            
            do
            {
                using var client = _httpClientFactory.CreateClient();
                var result = await client.GetAsync(url);
                resultString = await result.Content.ReadAsStringAsync();
            
            } while (string.IsNullOrWhiteSpace(resultString) && numberOfRetries++ < maxNumberOfRetries);
            
            if (string.IsNullOrWhiteSpace(resultString))
            {
                throw new InvalidOperationException($"Unable to fetch locked users from {url}");
            }
            var resultDict = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(resultString);
            return resultDict;
        }
    }
}