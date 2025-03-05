using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Datahub.Application.Configuration;
using Datahub.Core.Model;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Health;
using Datahub.Shared.Configuration;
using Azure.Core;
using System.Net.Http.Headers;
using Datahub.Core.Data;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph.Models;

namespace Datahub.Portal.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/healthcheck")]
    public class HealthCheckController : ControllerBase
    {
        private readonly DatahubProjectDBContext _dbProjectContext;
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly DatahubPortalConfiguration _portalConfiguration;

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks =
            new ConcurrentDictionary<string, SemaphoreSlim>();


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
                if (!_cache.TryGetValue("InfrastructureHealthChecks",
                        out List<InfrastructureHealthCheck> infrastructureHealth))
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

        [HttpGet("webapplogstream")]
        [Authorize]
        [AllowIFrame]
        public async Task<IActionResult> GetKuduLogStreamForUser([FromQuery] string ws)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated. L'utilisateur n'est pas authentifié.");
            }

            if (string.IsNullOrEmpty(ws))
            {
                return BadRequest("Workspace acronym is missing. L'acronyme de l'espace de travail est manquant.");
            }

            // Check if the user has the admin, workspace lead or datahub admin role for the specified workspace
            var allowed = User.Claims.Any(
                claim =>
                    claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" &&
                    (claim.Value == $"{ws}{RoleConstants.ADMIN_SUFFIX}" ||
                     claim.Value == $"{ws}{RoleConstants.WORKSPACE_LEAD_SUFFIX}") ||
                    claim.Value == RoleConstants.DATAHUB_ROLE_ADMIN);

            if (!allowed)
            {
                return Unauthorized(
                    "User is not authorized to view web app logs for this workspace. L'utilisateur n'est pas autorisé à consulter les journaux de l'application Web pour cet espace de travail.");
            }

            var env = _portalConfiguration?.Hosting?.EnvironmentName;
            if (string.IsNullOrEmpty(env) || env == "local")
            {
                env = "dev";
            }

            var kuduUrl = $"https://fsdh-proj-{ws.ToLower()}-webapp-{env}.scm.azurewebsites.net/api/logstream";
            var access_token = await GetAccessTokenAsync();
            //await HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(access_token))
            {
                return Unauthorized("Access token is missing. Le jeton d'accès est manquant.");
            }

            var request = new HttpRequestMessage(HttpMethod.Get, kuduUrl);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            try
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    Response.ContentType = "text/event-stream";

                    using (var streamReader = new StreamReader(stream))
                    using (var writer = new StreamWriter(Response.Body))
                    {
                        var buffer = new char[8192];
                        int charsRead;
                        while ((charsRead = await streamReader.ReadAsync(buffer, 0, buffer.Length)) > 0 &&
                               charsRead > 0)
                        {
                            await writer.WriteAsync(buffer, 0, charsRead);
                            await writer.FlushAsync();
                        }
                    }

                    return new EmptyResult();
                    // [VB] could not use FileStreamResult - it did not stream as supposed to
                    //return new FileStreamResult(stream, new MediaTypeHeaderValue("text/event-stream").MediaType); 
                }

                return StatusCode((int)response.StatusCode,
                    $"Failed to get log stream. Échec de l'obtention du flux de journal. {response.ReasonPhrase}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Failed to get log stream. Échec de l'obtention du flux de journal.");
            }
        }

        private async Task<List<InfrastructureHealthCheck>> LoadCheckData()
        {
            var projects = await _dbProjectContext.Projects.AsNoTracking().Include(p => p.Resources)
                .Select(p => p.Project_Acronym_CD).ToListAsync();
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
            var tokenResponse =
                await _httpClient.PostAsync<AccessTokenResponse>(url, default, content, cancellationToken);
            return tokenResponse?.access_token;
        }
    }
}