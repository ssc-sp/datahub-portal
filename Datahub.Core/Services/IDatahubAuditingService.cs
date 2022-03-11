using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public enum AuditChangeType : int
    {
        New,
        Edit,
        Delete,
        Download
    }

    public interface IDatahubAuditingService
    {
        /// <summary>
        /// Track data change event.
        /// </summary>
        /// <param name="objectId">Changed object identifier (ID)</param>
        /// <param name="table">Data table where the the change occurred</param>
        /// <param name="changeType">Change type: New, Edit or Delete</param>
        /// <param name="anonymous">Log an anonymous operation (e.g. public download) instead of detecting a logged-in user</param>
        /// <param name="details">Extra details (optional)</param>
        Task TrackDataEvent(string objectId, string table, AuditChangeType changeType, bool anonymous, params (string key, string value)[] details);

        /// <summary>
        /// Track data change event.
        /// </summary>
        /// <param name="objectId">Changed object identifier (ID)</param>
        /// <param name="table">Data table where the the change occurred</param>
        /// <param name="changeType">Change type: New, Edit or Delete</param>
        /// <param name="details">Extra details (optional)</param>
        Task TrackDataEvent(string objectId, string table, AuditChangeType changeType, params (string key, string value)[] details) => 
            TrackDataEvent(objectId, table, changeType, false, details);


        /// <summary>
        /// Track security related change event.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="table">Data table where the the change occurred</param>
        /// <param name="changeType">Change type: New, Edit or Delete</param>
        /// <param name="details">Extra details (optional)</param>
        Task TrackSecurityEvent(string scope, string table, AuditChangeType changeType, params (string key, string value)[] details);

        /// <summary>
        /// Track administration related change event.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="source">Source of the change Class/Table</param>
        /// <param name="changeType">Change type: New, Edit or Delete</param>
        /// <param name="details">Extra details (optional)</param>
        Task TrackAdminEvent(string scope, string source, AuditChangeType changeType, params (string key, string value)[] details);

        /// <summary>
        /// Tracks and exception
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        Task TrackException(Exception exception, params (string key, string value)[] details);
    }

    public static class IDatahubAuditingServiceExtensions
    {
        /// <summary>
        /// Saves the changes async and tracks the changes as data change events
        /// </summary>
        public static async Task<int> TrackSaveChangesAsync(this DbContext dbContext, IDatahubAuditingService auditService, bool anonymous = false)
        {
            try
            {
                dbContext.ChangeTracker.DetectChanges();

                foreach (var entry in dbContext.ChangeTracker.Entries())
                {
                    if (entry.State == EntityState.Added)
                        await auditService.TrackDataEvent(
                            entry.DebugView.ShortView, 
                            entry.Entity.GetType().Name, 
                            AuditChangeType.New,
                            anonymous);

                    if (entry.State == EntityState.Modified)
                        await auditService.TrackDataEvent(
                            entry.DebugView.ShortView, 
                            entry.Entity.GetType().Name, 
                            AuditChangeType.Edit,
                            anonymous,
                            ("changeDetails", entry.DebugView.LongView));

                    if (entry.State == EntityState.Deleted)
                        await auditService.TrackDataEvent(
                            entry.DebugView.ShortView, 
                            entry.Entity.GetType().Name, 
                            AuditChangeType.Delete,
                            anonymous);
                }
            }
            catch (Exception ex)
            {
                await auditService.TrackException(ex);
            }

            return await dbContext.SaveChangesAsync();
        }
    }
}
