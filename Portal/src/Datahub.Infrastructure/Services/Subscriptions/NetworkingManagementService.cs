using Datahub.Application.Services.Subscriptions;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.Subscriptions;

public class NetworkingManagementService(IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
    : INetworkingManagementService
{
    public async Task<List<VNet>> ListVNetsAsync(int? subscriptionId = null)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var query = ctx.VNets.Include(v => v.Subnets).AsNoTracking();
        if (subscriptionId is int subId)
            query = query.Where(v => v.SubscriptionId == subId);
        return await query.ToListAsync();
    }

    public async Task<List<Subnet>> ListSubnetsAsync(int vnetId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        return await ctx.Subnets
            .AsNoTracking()
            .Where(s => s.VNetId == vnetId)
            .ToListAsync();
    }

    public async Task AddVNetAsync(VNet vnet)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.VNets.Add(vnet);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateVNetAsync(VNet vnet)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.VNets.Update(vnet);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteVNetAsync(int vnetId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var vnet = await ctx.VNets.FindAsync(vnetId);
        if (vnet is not null)
        {
            ctx.VNets.Remove(vnet);
            await ctx.SaveChangesAsync();
        }
    }

    public async Task AddSubnetAsync(Subnet subnet)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.Subnets.Add(subnet);
        await ctx.SaveChangesAsync();
    }

    public async Task UpdateSubnetAsync(Subnet subnet)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        ctx.Subnets.Update(subnet);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteSubnetAsync(int subnetId)
    {
        await using var ctx = await dbContextFactory.CreateDbContextAsync();
        var subnet = await ctx.Subnets.FindAsync(subnetId);
        if (subnet is not null)
        {
            ctx.Subnets.Remove(subnet);
            await ctx.SaveChangesAsync();
        }
    }
}
