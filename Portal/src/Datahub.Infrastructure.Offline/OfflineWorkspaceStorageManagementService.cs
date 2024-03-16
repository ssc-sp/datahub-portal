using Datahub.Application.Services.Storage;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceStorageManagementService : IWorkspaceStorageManagementService
    {
        public Task<double> GetStorageCapacity(string workspaceAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<double> UpdateStorageCapacity(string workspaceAcronym)
        {
            throw new NotImplementedException();
        }
    }
}