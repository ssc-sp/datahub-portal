namespace Datahub.Application.Services;

/// <summary>
/// Manages the pool of Azure subnets available for Protected B workspace provisioning.
/// Each Protected B workspace is assigned a dedicated subnet group (8 subnets) from the pool.
/// The App Service subnet from the group is used for VNet integration on the Azure Linux Web App.
/// </summary>
public interface ISubnetPoolService
{
    /// <summary>
    /// Returns the full Azure resource ID of the App Service subnet assigned to the workspace,
    /// claiming the next available subnet group from the pool if none is currently assigned.
    /// </summary>
    /// <param name="projectId">The <c>Datahub_Project.Project_ID</c> of the workspace.</param>
    /// <param name="azureSubscriptionId">
    /// The Azure subscription ID string identifying which VNet pool to draw from.
    /// </param>
    /// <returns>
    /// Full ARM subnet resource ID (e.g.
    /// <c>/subscriptions/{subId}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{name}</c>),
    /// or <c>null</c> if no subnet is available in the pool.
    /// </returns>
    Task<string?> ClaimOrGetAppServiceSubnetIdAsync(int projectId, string azureSubscriptionId);
}
