using Datahub.Application.Exceptions;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;

namespace Datahub.Application.Services;

/// <summary>
/// Assigns subnet groups to Protected B workspaces. Each Protected B workspace gets its own subnet group
/// (the set of subnets sharing a group number) from the <c>Subnets</c> table, recorded in <c>WorkspaceSubnets</c>.
/// </summary>
public interface ISubnetPoolService
{
    /// <summary>
    /// Assigns the lowest available subnet group to a workspace. The assignment is added to <paramref name="db"/>
    /// but not saved, so the caller saves it together with the workspace.
    /// </summary>
    /// <param name="db">The context the workspace is being saved with.</param>
    /// <param name="project">The workspace to assign the group to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The group's subnet record.</returns>
    /// <exception cref="NoSubnetGroupAvailableException">No subnet group is available.</exception>
    Task<Subnet> ClaimSubnetGroupAsync(DatahubProjectDBContext db, Datahub_Project project,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the usage of each subnet group in a VNet.
    /// </summary>
    /// <param name="vnetId">The <see cref="VNet.Id"/> of the VNet.</param>
    /// <returns>The usage of each subnet group, keyed by <see cref="Subnet.Id"/>.</returns>
    Task<IReadOnlyDictionary<int, SubnetGroupUsage>> GetSubnetGroupUsageAsync(int vnetId);

    /// <summary>
    /// Manually assigns an available subnet group to a workspace, for example to record a workspace that was
    /// deployed on that group.
    /// </summary>
    /// <param name="subnetId">The <see cref="Subnet.Id"/> of the group.</param>
    /// <param name="workspaceAcronym">The acronym of the workspace.</param>
    /// <exception cref="InvalidOperationException">
    /// The group isn't available, or the workspace doesn't exist, is deleted, or already holds a group.
    /// </exception>
    Task AttachSubnetGroupAsync(int subnetId, string workspaceAcronym);

    /// <summary>
    /// Manually releases a subnet group so it becomes available straight away, removing its workspace assignments.
    /// </summary>
    /// <param name="subnetId">The <see cref="Subnet.Id"/> of the group.</param>
    Task ReleaseSubnetGroupAsync(int subnetId);
}

/// <summary>
/// Whether a subnet group can be assigned to a new Protected B workspace.
/// </summary>
public enum SubnetGroupStatus
{
    /// <summary>No workspace holds the group.</summary>
    Available,

    /// <summary>A live workspace holds the group.</summary>
    InUse,

    /// <summary>The group's workspace was deleted less than the cooldown ago.</summary>
    CoolingDown
}

/// <summary>
/// The usage of a subnet group.
/// </summary>
/// <param name="Status">Whether the group can be assigned.</param>
/// <param name="WorkspaceAcronym">The workspace holding the group, or the last one when cooling down; null when available.</param>
/// <param name="AvailableAtUtc">When a cooling-down group becomes available; otherwise null.</param>
public record SubnetGroupUsage(SubnetGroupStatus Status, string? WorkspaceAcronym = null, DateTime? AvailableAtUtc = null);
