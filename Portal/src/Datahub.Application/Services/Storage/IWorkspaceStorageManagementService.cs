namespace Datahub.Application.Services.Storage
{
    public interface IWorkspaceStorageManagementService
    {
        /// <summary>
        /// Queries the monitoring metrics of a storage account to get the used capacity
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronymm</param>
        /// <returns></returns>
        public Task<double> GetStorageCapacity(string workspaceAcronym);
        public Task<double> UpdateStorageCapacity(string workspaceAcronym);
    }
}