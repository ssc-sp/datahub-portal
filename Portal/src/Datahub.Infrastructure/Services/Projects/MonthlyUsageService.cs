using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Datahub.Infrastructure.Services.Projects;

public class MonthlyUsageService
{
	private readonly AzureManagementService _azureManagementService;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public MonthlyUsageService(AzureManagementService azureManagementService, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
	{
        _azureManagementService = azureManagementService;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Project_MonthlyUsage> GetProjectMonthlyUsage(int projectId, CancellationToken ct)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync();

        // find existing 
        var monthlyUsage = await ctx.Project_MonthlyUsage.FirstOrDefaultAsync(e => e.ProjectId == projectId);

        // create if it does exists
        monthlyUsage ??= new() { ProjectId = projectId };

        // get the resource names for the project
        var resourceNames = await GetAzureResourceNames(ctx, projectId);
        if (resourceNames is null)
            return monthlyUsage;

        // check the last update
        var today = DateTime.UtcNow.Date;
        if (monthlyUsage.LastUpdate.HasValue && today <= monthlyUsage.LastUpdate.Value)
            return monthlyUsage;

        // update usage
        await UpdateUsage(monthlyUsage, resourceNames.resource_group_name, resourceNames.storage_account);

        // add or update
        if (monthlyUsage.Id == 0)
        {
            ctx.Project_MonthlyUsage.Add(monthlyUsage);
        }
        else
        {
            ctx.Project_MonthlyUsage.Update(monthlyUsage);
        }

        // save changes
        await ctx.SaveChangesAsync();

        return monthlyUsage;
    }

    private async Task UpdateUsage(Project_MonthlyUsage monthlyUsage, string rg, string storage)
    {
        var session = await _azureManagementService.GetSession(default);

        // current monthly cost
        monthlyUsage.CurrentCost = await session.GetResourceGroupMonthlyCost(rg) ?? 0;

        // monthy by service
        var costByService = await session.GetResourceGroupMonthlyCostByService(rg);
        monthlyUsage.CurrentCostPerService = JsonSerializer.Serialize(costByService);

        // yesterday cost
        monthlyUsage.YesterdayCost = await session.GetResourceGroupYesterdayCost(rg) ?? 0;

        // yesterday by service
        var yesterdayCostByService = await session.GetResourceGroupYesterdayCostByService(rg);
        monthlyUsage.YesterdayCostPerService = JsonSerializer.Serialize(yesterdayCostByService);

        // current cost by day
        var costsPerDay = await session.GetResourceGroupMonthlyCostPerDay(rg);
        monthlyUsage.CurrentCostPerDay = JsonSerializer.Serialize(costsPerDay);

        // storage usage
        var usageDouble = await session.GetStorageUsedCapacity(rg, storage) ?? 0;
        monthlyUsage.CurrentStorageUsage = Convert.ToInt64(usageDouble);

        // last updated
        monthlyUsage.LastUpdate = DateTime.UtcNow.Date;
    }

    static async Task<ProjectResourceNames?> GetAzureResourceNames(DatahubProjectDBContext ctx, int projectId)
    {
        // >> review this!
        var resources = await ctx.Project_Resources2.Where(e => e.ProjectId == projectId).ToListAsync();
        return resources.Select(r => GetProjectResourceJSON(r.JsonContent)).FirstOrDefault(r => !string.IsNullOrEmpty(r?.resource_group_name));
    }

    static ProjectResourceNames? GetProjectResourceJSON(string data)
    {
        try
        {
            return JsonSerializer.Deserialize<ProjectResourceNames>(data ?? "")!;
        }
        catch
        {
            return default;
        }
    }

    record ProjectResourceNames(string resource_group_name, string storage_account);
}
