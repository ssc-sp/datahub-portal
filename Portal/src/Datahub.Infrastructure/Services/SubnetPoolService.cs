using Datahub.Application.Services;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services;

/// <summary>
/// Manages assignment of subnet groups from the VNet pool to Protected B workspaces.
/// Each call to <see cref="ClaimOrGetAppServiceSubnetIdAsync"/> is idempotent: if the workspace
/// already has a subnet group assigned it returns the existing App Service subnet ID.
/// </summary>
public class SubnetPoolService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory) : ISubnetPoolService
{
    // TODO: PLACEHOLDER — update this suffix to match the actual naming convention used in the
    //       Protected B subnet pool for subnets delegated to Microsoft.Web/serverFarms.
    //       e.g. "snet-app", "-app-snet", "webapp-subnet" depending on the IaC naming scheme.
    private const string AppServiceSubnetNameSuffix = "-app-snet";

    /// <inheritdoc />
    public async Task<string?> ClaimOrGetAppServiceSubnetIdAsync(int projectId, string azureSubscriptionId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();

        // Check if this workspace already has an App Service subnet assigned.
        var existingSubnet = await ctx.WorkspaceSubnets
            .Include(ws => ws.Subnet)
                .ThenInclude(s => s.VNet)
                    .ThenInclude(v => v.Subscription)
            .Where(ws => ws.ProjectId == projectId
                      && ws.Subnet.VNet.Subscription.SubscriptionId == azureSubscriptionId
                      && ws.Subnet.SubnetName.EndsWith(AppServiceSubnetNameSuffix))
            .Select(ws => ws.Subnet)
            .FirstOrDefaultAsync();

        if (existingSubnet is not null)
        {
            return BuildSubnetArmId(existingSubnet);
        }

        // Find the set of subnet group numbers already assigned to any workspace in this subscription.
        var assignedSubnetGroups = await ctx.WorkspaceSubnets
            .Include(ws => ws.Subnet)
                .ThenInclude(s => s.VNet)
                    .ThenInclude(v => v.Subscription)
            .Where(ws => ws.Subnet.VNet.Subscription.SubscriptionId == azureSubscriptionId)
            .Select(ws => ws.Subnet.SubnetGroup)
            .Distinct()
            .ToListAsync();

        // Find the next available App Service subnet whose group is not yet claimed.
        var availableSubnet = await ctx.Subnets
            .Include(s => s.VNet)
                .ThenInclude(v => v.Subscription)
            .Where(s => s.VNet.Subscription.SubscriptionId == azureSubscriptionId
                     && s.SubnetName.EndsWith(AppServiceSubnetNameSuffix)
                     && !assignedSubnetGroups.Contains(s.SubnetGroup))
            .FirstOrDefaultAsync();

        if (availableSubnet is null)
        {
            // TODO: PLACEHOLDER — emit an alert/metric for subnet pool exhaustion so that
            //       the infrastructure team can extend the pool before provisioning fails.
            return null;
        }

        // Assign all subnets in the group to this workspace.
        // NOTE: This is not atomic under concurrent load. A distributed lock or serializable
        //       transaction should be used if simultaneous PB workspace provisioning is expected.
        var groupSubnets = await ctx.Subnets
            .Where(s => s.VNetId == availableSubnet.VNetId
                     && s.SubnetGroup == availableSubnet.SubnetGroup)
            .ToListAsync();

        foreach (var subnet in groupSubnets)
        {
            ctx.WorkspaceSubnets.Add(new WorkspaceSubnet
            {
                ProjectId = projectId,
                SubnetId = subnet.Id
            });
        }

        await ctx.SaveChangesAsync();

        return BuildSubnetArmId(availableSubnet);
    }

    /// <summary>
    /// Constructs the full Azure ARM resource ID for a subnet from its entity.
    /// </summary>
    private static string BuildSubnetArmId(Subnet subnet) =>
        $"{subnet.VNet.VNetId}/subnets/{subnet.SubnetName}";
}
