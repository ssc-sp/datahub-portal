using Datahub.Core.Model.Workspace;

namespace Datahub.Application.Services.Storage
{
    public interface IDatabaseManagementService
    {
        /// <summary>
        /// Queries the metrics of a database
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns></returns>
        public Task<DatabaseInfo> GetDatabaseInfo(string workspaceAcronym);
    }
}