using Datahub.Application.Services;
using Datahub.Core.Model;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Datahub.Infrastructure.Services;

public class MiscStorageService : IMiscStorageService
{
    private readonly IDbContextFactory<DatahubProjectDBContext> dbContextFactory;
    private readonly ILogger<MiscStorageService> logger;
    private readonly IDatahubAuditingService auditingService;

    public MiscStorageService(
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        ILogger<MiscStorageService> logger,
        IDatahubAuditingService auditingService)
    {
        this.dbContextFactory = dbContextFactory;
        this.logger = logger;
        this.auditingService = auditingService;
    }

    private static string GetTypeName<T>() => typeof(T).ToString();

    private static MiscStoredObject CreateGenericObject<T>(T obj, string id) => new()
    {
        GeneratedId = Guid.NewGuid(),
        TypeName = GetTypeName<T>(),
        Id = id,
        JsonContent = JsonConvert.SerializeObject(obj)
    };

    private static async Task<MiscStoredObject> GetRawObject<T>(DatahubProjectDBContext ctx, string id) => await ctx.MiscStoredObjects
        .FirstOrDefaultAsync(e => e.TypeName == GetTypeName<T>() && e.Id == id);

    private static async Task<IEnumerable<MiscStoredObject>> GetAllRawObjects<T>(DatahubProjectDBContext ctx) => await ctx.MiscStoredObjects
        .Where(e => e.TypeName == GetTypeName<T>())
        .ToListAsync();

    public async Task<T> GetObject<T>(string id)
    {
        using var ctx = dbContextFactory.CreateDbContext();

        var rawObject = await GetRawObject<T>(ctx, id);

        if (rawObject == null)
        {
            return default;
        }

        return JsonConvert.DeserializeObject<T>(rawObject.JsonContent);
    }

    public async Task<IEnumerable<T>> GetAllObjects<T>()
    {
        using var ctx = dbContextFactory.CreateDbContext();

        var rawObjects = await GetAllRawObjects<T>(ctx);
        return rawObjects
            .Select(r => JsonConvert.DeserializeObject<T>(r.JsonContent));
    }

    private void VerifyObject<T>(MiscStoredObject obj)
    {
        try
        {
            _ = JsonConvert.DeserializeObject<T>(obj.JsonContent);
        }
        catch (JsonSerializationException ex)
        {
            logger.LogError(ex, "Type {} cannot be deserialized.", GetTypeName<T>());
            throw;
        }
    }

    public async Task SaveObject<T>(T obj, string id)
    {
        using var ctx = dbContextFactory.CreateDbContext();

        var genericObject = await GetRawObject<T>(ctx, id);

        if (genericObject == null)
        {
            genericObject = CreateGenericObject(obj, id);
            VerifyObject<T>(genericObject);
            ctx.MiscStoredObjects.Add(genericObject);
        }
        else
        {
            genericObject.JsonContent = JsonConvert.SerializeObject(obj);
            VerifyObject<T>(genericObject);
            ctx.MiscStoredObjects.Update(genericObject);
        }

        await ctx.TrackSaveChangesAsync(auditingService);
    }

    public async Task SaveObjects<T>(IEnumerable<T> objects, Func<T, string> idGenerator)
    {
        using var ctx = dbContextFactory.CreateDbContext();

        var allExistingObjects = (await GetAllRawObjects<T>(ctx)).ToDictionary(o => o.Id);

        var updatedObjects = objects
            .Where(o => allExistingObjects.ContainsKey(idGenerator(o)))
            .Select(o =>
            {
                var genericObj = allExistingObjects[idGenerator(o)];
                genericObj.JsonContent = JsonConvert.SerializeObject(o);
                VerifyObject<T>(genericObj);
                return genericObj;
            });

        var createdObjects = objects
            .Where(o => !allExistingObjects.ContainsKey(idGenerator(o)))
            .Select(o =>
            {
                var genericObj = CreateGenericObject(o, idGenerator(o));
                VerifyObject<T>(genericObj);
                return genericObj;
            });

        ctx.MiscStoredObjects.UpdateRange(updatedObjects);
        ctx.MiscStoredObjects.AddRange(createdObjects);

        await ctx.TrackSaveChangesAsync(auditingService);
    }
}