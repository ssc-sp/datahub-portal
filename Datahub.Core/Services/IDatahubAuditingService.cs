using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public enum AuditChangeType : int
    {
        New,
        Edit,
        Delete
    }

    public interface IDatahubAuditingService
    {
        /// <summary>
        /// Track data change event.
        /// </summary>
        /// <param name="objectId">Changed object identifier (ID)</param>
        /// <param name="table">Data table where the the change occurred</param>
        /// <param name="changeType">Change type: New, Edit or Delete</param>
        /// <param name="details">Extra details (optional)</param>
        Task TrackDataEvent(string objectId, string table, AuditChangeType changeType, params (string key, string value)[] details);

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
    }
}
