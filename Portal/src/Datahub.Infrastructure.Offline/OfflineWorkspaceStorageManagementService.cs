using Azure.Core;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceStorageManagementService : IWorkspaceStorageManagementService
    {
        public Task<double> GetStorageCapacity(string workspaceAcronym, List<string>? storageAccountId = null)
        {
            throw new NotImplementedException();
        }

        public bool CheckUpdateNeeded(string workspaceAcronym, DatahubProjectDBContext ctx)
        {
            throw new NotImplementedException();
        }

        public Task<string?> MoveBlobToUsersContainerAsync(string scannedFileUri, TokenCredential credential)
        {
            throw new NotImplementedException();
        }

        Task<double?> IWorkspaceStorageManagementService.UpdateStorageCapacity(string workspaceAcronym, List<string>? storageAccountId)
        {
            throw new NotImplementedException();
        }
    }
}
