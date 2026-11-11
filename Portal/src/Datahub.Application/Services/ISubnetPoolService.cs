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
    /// <remarks>
    /// A group is available when none of the workspaces assigned to it is live, and the last one was deleted
    /// longer ago than the configured cooldown.
    /// </remarks>
    /// <param name="db">The context the workspace is being saved with.</param>
    /// <param name="project">The workspace to assign the group to.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The group's subnet record.</returns>
    /// <exception cref="NoSubnetGroupAvailableException">No subnet group is available.</exception>
    Task<Subnet> ClaimSubnetGroupAsync(DatahubProjectDBContext db, Datahub_Project project,
        CancellationToken cancellationToken = default);
}
