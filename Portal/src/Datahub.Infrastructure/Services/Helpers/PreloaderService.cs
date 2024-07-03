using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datahub.Infrastructure.Services;

public class PreloaderService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMemoryCache _cache;

    public PreloaderService(IServiceScopeFactory serviceScopeFactory, IMemoryCache cache)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _cache = cache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatahubProjectDBContext>();

                var healthChecks = await dbContext.InfrastructureHealthChecks.ToListAsync();
                _cache.Set("InfrastructureHealthChecks", healthChecks);
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // call takes 10 sec
        }
    }
}