using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Azure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Datahub.Infrastructure.Services.Projects;

public class ProjectUsageService
{
	private readonly AzureManagementService _azureManagementService;
    private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

    public ProjectUsageService(AzureManagementService azureManagementService, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
	{
        _azureManagementService = azureManagementService;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Project_Credits> UpdateProjectUsage(int projectId, string resourceGroup, string storageAccount, CancellationToken ct)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);

        // find existing 
        var projectCredits = await ctx.Project_Credits.FirstOrDefaultAsync(e => e.ProjectId == projectId, ct);

        // create if it does exists
        projectCredits ??= new() { ProjectId = projectId };

        // check the last update
        var today = DateTime.UtcNow.Date;
        if (projectCredits.LastUpdate.HasValue && today <= projectCredits.LastUpdate.Value)
            return projectCredits;

        // update usage
        await UpdateUsage(projectCredits, resourceGroup, storageAccount, ct);

        // add or update
        if (projectCredits.Id == 0)
        {
            ctx.Project_Credits.Add(projectCredits);
        }
        else
        {
            ctx.Project_Credits.Update(projectCredits);
        }

        // save changes
        await ctx.SaveChangesAsync(ct);

        return projectCredits;
    }

    private async Task UpdateUsage(Project_Credits projectCredits, string rg, string storage, CancellationToken ct)
    {
        var session = await _azureManagementService.GetSession(ct);

        // current monthly cost
        projectCredits.Current = await session.GetResourceGroupMonthlyCost(rg) ?? 0;

        // monthy by service
        var costByService = await session.GetResourceGroupMonthlyCostByService(rg);
        projectCredits.CurrentPerService = JsonSerializer.Serialize(costByService);

        // yesterday cost
        projectCredits.YesterdayCredits = await session.GetResourceGroupYesterdayCost(rg) ?? 0;

        // yesterday by service
        var yesterdayCostByService = await session.GetResourceGroupYesterdayCostByService(rg);
        projectCredits.YesterdayPerService = JsonSerializer.Serialize(yesterdayCostByService);

        // current cost by day
        var costsPerDay = await session.GetResourceGroupMonthlyCostPerDay(rg);
        projectCredits.CurrentPerDay = JsonSerializer.Serialize(costsPerDay);

        // last updated
        projectCredits.LastUpdate = DateTime.UtcNow.Date;
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
