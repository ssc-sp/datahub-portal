using Datahub.Core.Model.Subscriptions;

namespace Datahub.Application.Services.Subscriptions;

public interface INetworkingManagementService
{
    /// <summary>Returns all VNets, optionally filtered by subscription.</summary>
    Task<List<VNet>> ListVNetsAsync(int? subscriptionId = null);

    /// <summary>Returns all subnets for a given VNet.</summary>
    Task<List<Subnet>> ListSubnetsAsync(int vnetId);

    /// <summary>Adds a new VNet to the database.</summary>
    Task AddVNetAsync(VNet vnet);

    /// <summary>Updates an existing VNet.</summary>
    Task UpdateVNetAsync(VNet vnet);

    /// <summary>Deletes a VNet and its subnets.</summary>
    Task DeleteVNetAsync(int vnetId);

    /// <summary>Adds a new subnet to a VNet.</summary>
    Task AddSubnetAsync(Subnet subnet);

    /// <summary>Updates an existing subnet.</summary>
    Task UpdateSubnetAsync(Subnet subnet);

    /// <summary>Deletes a subnet.</summary>
    Task DeleteSubnetAsync(int subnetId);
}
