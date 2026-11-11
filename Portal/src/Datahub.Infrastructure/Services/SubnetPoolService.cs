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
public class SubnetPoolService(
    DatahubPortalConfiguration portalConfiguration,
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory) : ISubnetPoolService
{
    /// <inheritdoc />
    public async Task<Subnet> ClaimSubnetGroupAsync(DatahubProjectDBContext db, Datahub_Project project,
        CancellationToken cancellationToken = default)
    {
        var subnets = await db.Subnets
            .Include(s => s.WorkspaceSubnets).ThenInclude(ws => ws.Project)
            .OrderBy(s => s.SubnetGroup)
            .ToListAsync(cancellationToken);

        // Not atomic: two Protected B workspaces requested at the same moment could get the same group.
        // Accepted because Protected B workspaces are rarely created.
        var subnet = subnets.FirstOrDefault(s => GetUsage(s).Status == SubnetGroupStatus.Available);
        if (subnet is null)
        {
            throw new NoSubnetGroupAvailableException(
                $"No subnet group is available for Protected B workspace '{project.Project_Acronym_CD}'.");
        }

        db.WorkspaceSubnets.Add(new WorkspaceSubnet { Project = project, Subnet = subnet });
        return subnet;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, SubnetGroupUsage>> GetSubnetGroupUsageAsync(int vnetId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var subnets = await db.Subnets
            .AsNoTracking()
            .Include(s => s.WorkspaceSubnets).ThenInclude(ws => ws.Project)
            .Where(s => s.VNetId == vnetId)
            .ToListAsync();

        return subnets.ToDictionary(s => s.Id, GetUsage);
    }

    /// <inheritdoc />
    public async Task AttachSubnetGroupAsync(int subnetId, string workspaceAcronym)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var subnet = await db.Subnets
            .Include(s => s.WorkspaceSubnets).ThenInclude(ws => ws.Project)
            .SingleOrDefaultAsync(s => s.Id == subnetId)
            ?? throw new InvalidOperationException("The subnet group was not found.");

        if (GetUsage(subnet).Status != SubnetGroupStatus.Available)
        {
            throw new InvalidOperationException($"Subnet group {subnet.SubnetGroup} is not available.");
        }

        var acronym = workspaceAcronym.Trim().ToUpperInvariant();
        var project = await db.Projects.SingleOrDefaultAsync(p => p.Project_Acronym_CD == acronym)
            ?? throw new InvalidOperationException($"Workspace '{acronym}' was not found.");

        if (project.Deleted_DT is not null)
        {
            throw new InvalidOperationException($"Workspace '{acronym}' is deleted.");
        }

        var heldGroup = await db.WorkspaceSubnets
            .Where(ws => ws.ProjectId == project.Project_ID)
            .Select(ws => (int?)ws.Subnet.SubnetGroup)
            .FirstOrDefaultAsync();
        if (heldGroup is not null)
        {
            throw new InvalidOperationException($"Workspace '{acronym}' already has subnet group {heldGroup}.");
        }

        db.WorkspaceSubnets.Add(new WorkspaceSubnet { ProjectId = project.Project_ID, SubnetId = subnet.Id });
        await db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task ReleaseSubnetGroupAsync(int subnetId)
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var assignments = await db.WorkspaceSubnets.Where(ws => ws.SubnetId == subnetId).ToListAsync();
        db.WorkspaceSubnets.RemoveRange(assignments);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Works out a group's usage from the workspaces assigned to it. A deleted workspace keeps its group until
    /// the cooldown has passed, so the delete pipeline has time to remove its resources from the subnets.
    /// </summary>
    private SubnetGroupUsage GetUsage(Subnet subnet)
    {
        var workspaces = subnet.WorkspaceSubnets.Select(ws => ws.Project).ToList();

        var live = workspaces.FirstOrDefault(p => p.Deleted_DT is null);
        if (live is not null)
        {
            return new SubnetGroupUsage(SubnetGroupStatus.InUse, live.Project_Acronym_CD);
        }

        var lastDeleted = workspaces.MaxBy(p => p.Deleted_DT);
        if (lastDeleted?.Deleted_DT is { } deletedUtc)
        {
            var availableAtUtc = deletedUtc.AddMinutes(portalConfiguration.Hosting.ProtectedBSubnetGroupCooldownMinutes);
            if (availableAtUtc > DateTime.UtcNow)
            {
                return new SubnetGroupUsage(SubnetGroupStatus.CoolingDown, lastDeleted.Project_Acronym_CD, availableAtUtc);
            }
        }

        return new SubnetGroupUsage(SubnetGroupStatus.Available);
    }
}
