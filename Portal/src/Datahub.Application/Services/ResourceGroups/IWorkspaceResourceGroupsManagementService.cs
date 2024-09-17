using Azure.Core;

namespace Datahub.Application.Services.ResourceGroups
{
    public interface IWorkspaceResourceGroupsManagementService
    {
        /// <summary>
        /// Gets the list of resource group names that belong to the given workspace. This will use two different methods
        /// to find the workspace resource groups. First it will look in database for this information, if it is not found
        /// it will use Azure Management API to get the resource groups. If the resource groups are not found, it will return
        /// an empty list.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>A list of the resource group names that belong to the given workspace. Empty list if none were found</returns>
        public Task<List<string>> GetWorkspaceResourceGroupsAsync(string workspaceAcronym);

        /// <summary>
        /// Gets the list of resource group identifiers that belong to the given workspace. This will use two different methods
        /// to find the workspace resource groups. First it will look in database for this information, if it is not found
        /// it will use Azure Management API to get the resource groups. If the resource groups are not found, it will return
        /// an empty list.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>A list of ResourceIdentifier for resource groups that belong to the given workspace. Empty list if none were found</returns>
        public Task<List<ResourceIdentifier>> GetWorkspaceResourceGroupsIdentifiersAsync(string workspaceAcronym);

        /// <summary>
        /// Gets the list of resource group names that belong to the given subscription. This will filter only workspace resource groups.
        /// </summary>
        /// <param name="subscriptionId">The subscription id to search resource groups in</param>
        /// <returns>List of resource groups in this subscription</returns>
        public Task<List<string>> GetAllSubscriptionResourceGroupsAsync(string subscriptionId);

        /// <summary>
        /// Gets the list of all resource groups in all subscriptions. This will filter only workspace resource groups.
        /// </summary>
        /// <returns>List of resource groups in this subscription</returns>
        public Task<List<string>> GetAllResourceGroupsAsync();
    }
}