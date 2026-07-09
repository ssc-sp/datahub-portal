using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Subscriptions;

/// <summary>
/// Represents the association between a workspace (<see cref="Datahub_Project"/>) and a subnet.
/// A workspace is associated with 8 subnets that share the same subnet group number.
/// </summary>
public class WorkspaceSubnet
{
    /// <summary>
    /// Gets or sets the unique identifier for this workspace-subnet mapping.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key referencing the workspace.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the workspace associated with this subnet.
    /// </summary>
    public Datahub_Project Project { get; set; } = null!;

    /// <summary>
    /// Gets or sets the foreign key referencing the subnet.
    /// </summary>
    public int SubnetId { get; set; }

    /// <summary>
    /// Gets or sets the subnet associated with this workspace.
    /// </summary>
    public Subnet Subnet { get; set; } = null!;
}
