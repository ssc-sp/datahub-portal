using Datahub.Application.Services.Notification;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace Datahub.Functions
{
    public class FunctionsHealthCheck
    {
        private const string CacheKey = "fsdh-dotnet-func-health-result";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly ILogger _logger;
        private readonly IGCNotifyService _gcNotify;
        private readonly IMemoryCache _cache;

        public FunctionsHealthCheck(ILoggerFactory loggerFactory, IGCNotifyService gCNotifyService, IMemoryCache cache)
        {
            _logger = loggerFactory.CreateLogger<FunctionsHealthCheck>();
            _gcNotify = gCNotifyService;
            _cache = cache;
        }

        [Function("FunctionsHealthCheck")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("Health check invoked.");

            bool cacheHit = _cache.TryGetValue<(bool Result, DateTimeOffset CheckedAt)>(CacheKey, out var cached);

            (bool result, DateTimeOffset checkedAt) data;
            if (cacheHit && DateTimeOffset.UtcNow - cached.CheckedAt < CacheDuration)
            {
                data = cached;
                _logger.LogDebug("Returning cached GC Notify health result (age {AgeSeconds:n0}s).",
                    (DateTimeOffset.UtcNow - cached.CheckedAt).TotalSeconds);
            }
            else
            {
                _logger.LogInformation("Cached result missing/expired. Executing GC Notify health check.");
                bool healthy = await _gcNotify.CheckHealthAsync();
                data = (healthy, DateTimeOffset.UtcNow);
                // cache (overwrite existing)
                _cache.Set(CacheKey, data, data.checkedAt.Add(CacheDuration));
            }

            var statusCode = data.result ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.Headers.Add("X-Cache", cacheHit ? "HIT" : "MISS");
            response.Headers.Add("X-Health-Checked-At", data.checkedAt.ToString("o"));
            response.Headers.Add("Cache-Control", "public, max-age=60"); // outward-facing (optional)
            await response.WriteStringAsync(data.result ? "Success!" : "Failed!");

            return response;
        }
    }
}
