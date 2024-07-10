using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Documentation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Functions;

public class DocumentationRankUpdate
{
    private readonly ILogger<DocumentationRankUpdate> _logger;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public DocumentationRankUpdate(ILoggerFactory loggerFactory, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    {
        _logger = loggerFactory.CreateLogger<DocumentationRankUpdate>();
        _dbContextFactory = dbContextFactory;
    }

    [Function("DocumentationRankUpdate")]
    public void RunCron([TimerTrigger("%DocumentationRankUpdateCRON%")] TimerInfo timerInfo)
    {
        UpdateRanking();
    }

    //[Function("DocumentationRankUpdateHttp")]
    //public IActionResult RunHttp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req)
    //{
    //    var ranking = UpdateRanking();
    //    var ordered = ranking.OrderByDescending(x => x.Value);
    //    return new OkObjectResult(ordered);
    //}

    private Dictionary<Guid, int> UpdateRanking()
    {
        using var ctx = _dbContextFactory.CreateDbContext();

        var ranking = new Dictionary<Guid, int>(250);

        // build ranking
        foreach (var docId in GetDocumentIds(ctx))
        {
            ranking.TryGetValue(docId, out var count);
            ranking[docId] = count + 1;
        }

        var existing = ctx.DocumentationResources.ToDictionary(e => e.Id);

        // update ranking
        foreach (var kv in ranking)
        {
            if (existing.TryGetValue(kv.Key, out var doc))
            {
                doc.Hits = kv.Value;
                doc.LastUpdated = DateTime.Now;
                ctx.DocumentationResources.Update(doc);
            }
            else
            {
                doc = new DocumentationResource()
                {
                    Id = kv.Key,
                    Hits = kv.Value,
                    LastUpdated = DateTime.Now
                };
                ctx.DocumentationResources.Add(doc);
            }
        }

        // save 
        ctx.SaveChanges();

        return ranking;
    }

    private IEnumerable<Guid> GetDocumentIds(DatahubProjectDBContext ctx)
    {
        var searchPath = "/resources/";
        var resPaths = ctx.TelemetryEvents.Where(e => e.EventName.StartsWith(searchPath)).Select(e => e.EventName);
        foreach (var path in resPaths)
        {
            var id = path[searchPath.Length..];
            if (Guid.TryParse(id, out var docId))
            {
                yield return docId;
            }
        }
    }
}