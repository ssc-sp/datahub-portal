using Amazon.Runtime.Internal.Util;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Datahub.Infrastructure.Services;

public class PreloaderService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PreloaderService> logger;
    private readonly AsyncRetryPolicy retryPolicy;

    public PreloaderService(IServiceScopeFactory serviceScopeFactory, IMemoryCache cache, ILogger<PreloaderService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _cache = cache;
        this.logger = logger;
        // Inside your method
        retryPolicy = Polly.Policy
            .Handle<Exception>() // Specify the exceptions you want to handle/retry. This example retries on any exception.
            .WaitAndRetryAsync(
                3, // Retry count
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential back-off. You can adjust the policy as needed.
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    // This is a callback you can use to log retries or take other actions.
                    logger.LogWarning(exception, $"Retry {retryCount} after {timeSpan.TotalSeconds} seconds");
                });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {



            await retryPolicy.ExecuteAsync(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<DatahubProjectDBContext>();

                    var healthChecks = await dbContext.InfrastructureHealthChecks.ToListAsync();
                    _cache.Set("InfrastructureHealthChecks", healthChecks);
                }
            });

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // call takes 10 sec
        }
    }
}