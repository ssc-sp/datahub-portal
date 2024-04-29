﻿namespace Datahub.Application.Services.Storage
{
    public interface IWorkspaceStorageManagementService
    {
        /// <summary>
        /// Queries the monitoring metrics of a storage account to get the used capacity
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="storageAccountId">Optional storage account ids to use. If not provided, will be interpolated</param>
        /// <returns></returns>
        public Task<double> GetStorageCapacity(string workspaceAcronym, List<string>? storageAccountId = null);
        /// <summary>
        /// Update the storage capacity of a workspace in database
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="storageAccountId">Optional storage account ids to use. If not provided, will be interpolated</param>
        /// <returns></returns>
        public Task<double> UpdateStorageCapacity(string workspaceAcronym, List<string>? storageAccountId = null);
    }
}