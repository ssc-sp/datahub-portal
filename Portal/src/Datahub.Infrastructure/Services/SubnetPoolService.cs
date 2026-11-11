using Datahub.Application.Configuration;
using Datahub.Application.Exceptions;
using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

/// <summary>
/// Assigns subnet groups to Protected B workspaces from the <c>Subnets</c> table.
/// </summary>
public class SubnetPoolService(DatahubPortalConfiguration portalConfiguration) : ISubnetPoolService
{
    /// <inheritdoc />
    public async Task<Subnet> ClaimSubnetGroupAsync(DatahubProjectDBContext db, Datahub_Project project,
        CancellationToken cancellationToken = default)
    {
        // A deleted workspace keeps its group until the cooldown has passed, so the delete pipeline has time
        // to remove its resources from the subnets.
        var cooldown = TimeSpan.FromMinutes(portalConfiguration.Hosting.ProtectedBSubnetGroupCooldownMinutes);
        var releasedBefore = DateTime.UtcNow - cooldown;

        // Not atomic: two Protected B workspaces requested at the same moment could get the same group.
        // Accepted because Protected B workspaces are rarely created.
        var subnet = await db.Subnets
            .Where(s => !s.WorkspaceSubnets.Any(ws =>
                ws.Project.Deleted_DT == null || ws.Project.Deleted_DT > releasedBefore))
            .OrderBy(s => s.SubnetGroup)
            .FirstOrDefaultAsync(cancellationToken);

        if (subnet is null)
        {
            throw new NoSubnetGroupAvailableException(
                $"No subnet group is available for Protected B workspace '{project.Project_Acronym_CD}'.");
        }

        db.WorkspaceSubnets.Add(new WorkspaceSubnet { Project = project, Subnet = subnet });
        return subnet;
    }
}
