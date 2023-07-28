using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Newtonsoft.Json;


namespace Datahub.Portal.Services
{
    public class UsersStatusService : IUsersStatusService
    {
        public async Task<Dictionary<string, List<string>>> GetUsersStatus(IHttpClientFactory _httpClientFactory, DatahubPortalConfiguration _datahubPortalConfiguration)
        {
            var url = _datahubPortalConfiguration.DatahubGraphLockedUsersUrl;
            
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