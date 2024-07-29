using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Microsoft.Identity.Client;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Health;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Azure.Core;
using System.Net.Http.Headers;


namespace Datahub.Portal.Controllers
{
    [ApiController]
    [Route("api/healthcheck")]
    public class HealthCheckController : ControllerBase
    {
        private readonly DatahubProjectDBContext _dbProjectContext;
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly DatahubPortalConfiguration _portalConfiguration;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();


        public HealthCheckController(DatahubProjectDBContext dbProjectContext, 
            IMemoryCache cache,
            DatahubPortalConfiguration portalConfiguration,
            HttpClient httpClient)
        {
            _dbProjectContext = dbProjectContext;
            _cache = cache;
            _portalConfiguration = portalConfiguration;
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealthCheckData()
        {
            var semaphore = _locks.GetOrAdd("InfrastructureHealthChecks", new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();

            try
            {
                if (!_cache.TryGetValue("InfrastructureHealthChecks", out List<InfrastructureHealthCheck> infrastructureHealth))
                {
                    infrastructureHealth = await LoadCheckData();
                    _cache.Set("InfrastructureHealthChecks", infrastructureHealth);
                }

                return Ok(infrastructureHealth);
            }
            finally
            {
                semaphore.Release();
            }
        }
         
        [HttpGet("logstream1")]
        public async Task<IActionResult> GetKuduLogStream()
        {
            
            var kuduUrl = $"https://fsdh-portal-app-dev.scm.azurewebsites.net/api/logstream";
            var request = new HttpRequestMessage(HttpMethod.Get, kuduUrl);

            var access_token = await GetUserAccessTokenAsync(); //GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Add("Bearer", access_token);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return new FileStreamResult(stream, response.Content.Headers.ContentType.ToString());
            }
            else
            {
                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
            }
            
        // var content = await response.Content.ReadAsStringAsync();
        // return Content(content, "text/html");
        }

        [HttpGet("logstream")]
        public async Task<IActionResult> GetLogStream()
        {
            var access_token = await GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Add("Bearer", access_token);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://fsdh-portal-app-dev.scm.azurewebsites.net/api/logstream");
            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "text/html");
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
            }
        }
        [HttpGet("logstream2")]
        public async Task<IActionResult> HealthCheck()
        {
            var kuduUrl = $"https://fsdh-portal-app-dev.scm.azurewebsites.net/api/logstream";
            var token = await GetUserAccessTokenAsync();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(kuduUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(content);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, response.ReasonPhrase);
                }
            }
        }

        private async Task<List<InfrastructureHealthCheck>> LoadCheckData()
        {
            var projects = await _dbProjectContext.Projects.AsNoTracking().Include(p => p.Resources).Select(p => p.Project_Acronym_CD).ToListAsync();
            var infrastructureHealth = await _dbProjectContext.InfrastructureHealthChecks
                .Where(h => h.ResourceType == InfrastructureHealthResourceType.AzureSqlDatabase ||
                            h.ResourceType == InfrastructureHealthResourceType.AzureDatabricks ||
                            h.ResourceType == InfrastructureHealthResourceType.AzureStorageAccount ||
                            h.ResourceType == InfrastructureHealthResourceType.AsureServiceBus ||
                            h.ResourceType == InfrastructureHealthResourceType.AzureWebApp)
                .ToListAsync();

            return infrastructureHealth;
        }

        public IConfidentialClientApplication GetClient(AzureDevOpsConfiguration config)
        {
            return ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{config.TenantId}"))
                .Build();
        }
        public async Task<string> GetAccessTokenAsync()
        {
            var scopes = "https://management.azure.com/.default";
            // Configure the Azure DevOps client
            var config = new AzureDevOpsConfiguration
            {
                TenantId = _portalConfiguration.AzureAd.TenantId,
                ClientId = _portalConfiguration.AzureAd.InfraClientId,
                ClientSecret = _portalConfiguration.AzureAd.InfraClientSecret
            };
            var _app = GetClient(config);
            var result = await _app.AcquireTokenForClient([scopes]).ExecuteAsync();

            return result.AccessToken;
        }
        
        public async Task<string> GetUserAccessTokenAsync()
        {
            var scopes = new[] { "https://management.azure.com/.default" };
            var credential = new InteractiveBrowserCredential();

            var tokenRequestContext = new TokenRequestContext(scopes);
            var token = await credential.GetTokenAsync(tokenRequestContext);

            return token.Token;
        }
    }
}