using Azure.Core;
using Datahub.Application.Services.ResourceGroups;

namespace Datahub.Infrastructure.Offline
{
    public class OfflineWorkspaceResourceGroupsManagementService : IWorkspaceResourceGroupsManagementService
    {
        public Task<List<string>> GetWorkspaceResourceGroupsAsync(string workspaceAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<List<ResourceIdentifier>> GetWorkspaceResourceGroupsIdentifiersAsync(string workspaceAcronym)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAllSubscriptionResourceGroupsAsync(string subscriptionId)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAllResourceGroupsAsync()
        {
            throw new NotImplementedException();
        }
    }
}