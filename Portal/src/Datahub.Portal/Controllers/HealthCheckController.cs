using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Datahub.Core.Model.Datahub;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.Model.Health;
using System.Collections.Concurrent;
using Datahub.Core.Model.Context;

namespace Datahub.Portal.Controllers
{
    [ApiController]
    [Route("api/healthcheck")]
    public class HealthCheckController : ControllerBase
    {
        private readonly DatahubProjectDBContext _dbProjectContext;
        private readonly IMemoryCache _cache;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();

        public HealthCheckController(DatahubProjectDBContext dbProjectContext, IMemoryCache cache)
        {
            _dbProjectContext = dbProjectContext;
            _cache = cache;
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
    }
}