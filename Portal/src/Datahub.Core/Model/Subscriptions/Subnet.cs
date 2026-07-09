namespace Datahub.Core.Model.Subscriptions;

/// <summary>
/// Represents an Azure subnet within a VNet.
/// A subnet can be mapped to one or more workspaces via <see cref="WorkspaceSubnet"/>.
/// </summary>
public class Subnet
{
    /// <summary>
    /// Gets or sets the unique identifier for this subnet record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the subnet (e.g. "GcDcCNR-SSC_FSDHWorkspace_PEP-1-snet").
    /// </summary>
    public required string SubnetName { get; set; }

    /// <summary>
    /// Gets or sets the address prefix of the subnet in CIDR notation (e.g. "10.0.1.64/28").
    /// </summary>
    public string? AddressPrefix { get; set; }

    /// <summary>
    /// Gets or sets the group number that identifies the set of 8 subnets assigned to a workspace.
    /// </summary>
    public int SubnetGroup { get; set; }

    /// <summary>
    /// Gets or sets the foreign key referencing the VNet that owns this subnet.
    /// </summary>
    public int VNetId { get; set; }

    /// <summary>
    /// Gets or sets the VNet that contains this subnet.
    /// </summary>
    public VNet VNet { get; set; } = null!;

    /// <summary>
    /// Gets or sets the workspace subnet mappings for this subnet.
    /// </summary>
    public List<WorkspaceSubnet> WorkspaceSubnets { get; set; } = new();
}
