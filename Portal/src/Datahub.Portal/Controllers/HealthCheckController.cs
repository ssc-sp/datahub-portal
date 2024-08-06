using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Datahub.Application.Configuration;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Health;
using Datahub.Shared.Configuration;
using Azure.Core;
using System.Net.Http.Headers;
using Datahub.Infrastructure.Services.Azure;


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
         
        [HttpGet("logstream")]
        public async Task<IActionResult> GetKuduLogStream()
        {
            var env = _portalConfiguration?.Hosting?.EnvironmentName;
            if (string.IsNullOrEmpty(env) || env == "local")
            {
                env = "dev";
            }

            var kuduUrl = $"https://fsdh-portal-app-{env}.scm.azurewebsites.net/api/logstream";
            var access_token = await GetAccessTokenAsync();            
            var request = new HttpRequestMessage(HttpMethod.Get, kuduUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                return new FileStreamResult(stream, response.Content.Headers.ContentType.ToString());
            }
            else
            {
                return StatusCode((int)response.StatusCode, response.ReasonPhrase);
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

        private async Task<string> GetAccessTokenAsync()
        {
            var config = new AzureDevOpsConfiguration
            {
                TenantId = _portalConfiguration.AzureAd.TenantId,
                ClientId = _portalConfiguration.AzureAd.ClientId,
                ClientSecret = _portalConfiguration.AzureAd.ClientSecret
            };

            var cancellationToken = CancellationToken.None;
            var audience = AzureManagementUrls.ManagementUrl;
            var url = $"{AzureManagementUrls.LoginUrl}/{config.TenantId}/oauth2/token";
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", config.ClientId },
                { "client_secret", config.ClientSecret },
                { "scope", $"{audience}.default" },
                { "resource", audience }
            });
            var tokenResponse = await _httpClient.PostAsync<AccessTokenResponse>(url, default, content, cancellationToken);
            return tokenResponse?.access_token;
        }
    }

    class AccessTokenResponse
    {
        public string? token_type { get; set; }
        public string? expires_in { get; set; }
        public string? ext_expires_in { get; set; }
        public string? expires_on { get; set; }
        public string? not_before { get; set; }
        public string? resource { get; set; }
        public string? access_token { get; set; }
    }
}