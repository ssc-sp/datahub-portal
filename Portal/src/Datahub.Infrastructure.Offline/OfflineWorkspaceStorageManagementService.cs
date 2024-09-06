using Datahub.Application.Services.Storage;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceStorageManagementService : IWorkspaceStorageManagementService
    {
        public Task<double> GetStorageCapacity(string workspaceAcronym, List<string>? storageAccountId = null)
        {
            throw new NotImplementedException();
        }

        public Task<double> UpdateStorageCapacity(string workspaceAcronym, List<string>? storageAccountId = null)
        {
            throw new NotImplementedException();
        }

        public bool CheckUpdateNeeded(string workspaceAcronym)
        {
            throw new NotImplementedException();
        }
    }
}