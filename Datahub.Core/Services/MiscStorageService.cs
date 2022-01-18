using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class MiscStorageService : IMiscStorageService
    {
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        private readonly ILogger<MiscStorageService> _logger;
        private readonly IDatahubAuditingService _auditingService;

        public MiscStorageService(
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
            ILogger<MiscStorageService> logger, 
            IDatahubAuditingService auditingService
            )
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _auditingService = auditingService;
        }

        private static MiscStoredObject CreateGenericObject<T>(T obj, string id) => new()
        {
            GeneratedId = Guid.NewGuid(),
            TypeName = typeof(T).FullName,
            Id = id,
            JsonContent = JsonConvert.SerializeObject(obj)
        };

        private static async Task<MiscStoredObject> GetRawObject<T>(DatahubProjectDBContext ctx, string id) => await ctx.MiscStoredObjects
            .FirstOrDefaultAsync(e => e.TypeName == typeof(T).FullName && e.Id == id);

        private static async Task<IEnumerable<MiscStoredObject>> GetAllRawObjects<T>(DatahubProjectDBContext ctx) => await ctx.MiscStoredObjects
            .Where(e => e.TypeName == typeof(T).FullName)
            .ToListAsync();

        public async Task<T> GetObject<T>(string id)
        {
            using var ctx = _dbContextFactory.CreateDbContext();

            var rawObject = await GetRawObject<T>(ctx, id);

            if (rawObject == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(rawObject.JsonContent);
        }

        public async Task<IEnumerable<T>> GetAllObjects<T>()
        {
            using var ctx = _dbContextFactory.CreateDbContext();

            var rawObjects = await GetAllRawObjects<T>(ctx);
            return rawObjects
                .Select(r => JsonConvert.DeserializeObject<T>(r.JsonContent));
        }

        private void VerifyObject<T>(MiscStoredObject obj)
        {
            try
            {
                var _ = JsonConvert.DeserializeObject<T>(obj.JsonContent);
            }
            catch (JsonSerializationException ex)
            {
                _logger.LogError(ex, "Type {} cannot be deserialized.", typeof(T).FullName);
                throw;
            }
        }

        public async Task SaveObject<T>(T obj, string id)
        {
            using var ctx = _dbContextFactory.CreateDbContext();

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

            await ctx.TrackSaveChangesAsync(_auditingService);
        }

        public async Task SaveObjects<T>(IEnumerable<T> objects, Func<T, string> idGenerator)
        {
            using var ctx = _dbContextFactory.CreateDbContext();

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

            await ctx.TrackSaveChangesAsync(_auditingService);
        }
    }
}
