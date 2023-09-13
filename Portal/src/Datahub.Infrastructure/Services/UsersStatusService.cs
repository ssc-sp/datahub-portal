using System.Net;
using Datahub.Application.Configuration;
using Datahub.Application.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;

namespace Datahub.Infrastructure.Services
{
    public partial class UsersStatusService : IUsersStatusService
    {
        private readonly ILogger<UsersStatusService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DatahubPortalConfiguration _datahubPortalConfiguration;
        static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
            Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .OrResult(x => x.StatusCode == HttpStatusCode.Conflict)
                .WaitAndRetryAsync(5, i => 
            TimeSpan.FromSeconds(1)
        );

        public UsersStatusService(ILogger<UsersStatusService> logger, IHttpClientFactory httpClientFactory,
            DatahubPortalConfiguration datahubPortalConfiguration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _datahubPortalConfiguration = datahubPortalConfiguration;
        }
        
        public async Task<Dictionary<string, List<string>>?> GetUsersStatus()
        {
            var url = _datahubPortalConfiguration.DatahubGraphUsersStatusFunctionUrl;
            
            var client = _httpClientFactory.CreateClient();
            var result = await _retryPolicy.ExecuteAsync(() => client.GetAsync(url));
            var resultString = await result.Content.ReadAsStringAsync();
            
            var resultDict = JsonConvert.DeserializeObject<Dictionary<string,List<string>>>(resultString);
            return resultDict;
        }
    }
}