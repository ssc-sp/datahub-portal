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

    public async Task<Project_Credits> UpdateProjectUsage(int projectId, string resourceGroup, CancellationToken ct)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);

        // find existing 
        var projectCredits = await ctx.Project_Credits.FirstOrDefaultAsync(e => e.ProjectId == projectId, ct);

        // create if it does exists
        projectCredits ??= new() { ProjectId = projectId };

        // check the last update
        //var today = DateTime.UtcNow.Date;
        //if (projectCredits.LastUpdate.HasValue && today <= projectCredits.LastUpdate.Value)
        //    return projectCredits;

        // update usage
        await UpdateUsage(projectCredits, resourceGroup, ct);

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

    private async Task UpdateUsage(Project_Credits projectCredits, string resourceGroup, CancellationToken ct)
    {
        var session = await _azureManagementService.GetSession(ct);

        // current monthly cost
        var yearCost = await session.GetResourceGroupLastYearCost(resourceGroup);
        if (yearCost == null)
            throw new Exception($"Failed to read up to date cost for resource group '{resourceGroup}'");

        projectCredits.Current = yearCost.Value;

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // monthy by service
        var costByService = await session.GetResourceGroupYearCostByService(resourceGroup);
        if (costByService is not null)
            projectCredits.CurrentPerService = JsonSerializer.Serialize(costByService);

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // yesterday cost
        var yesterdayCredits = await session.GetResourceGroupYesterdayCost(resourceGroup);
        if (yesterdayCredits.HasValue)
            projectCredits.YesterdayCredits = yesterdayCredits.Value;

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // yesterday by service
        var yesterdayCostByService = await session.GetResourceGroupYesterdayCostByService(resourceGroup);
        if (yesterdayCostByService is not null)
            projectCredits.YesterdayPerService = JsonSerializer.Serialize(yesterdayCostByService);

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // current cost by day
        var costsPerDay = await session.GetResourceGroupMonthlyCostPerDay(resourceGroup);
        if (costsPerDay is not null)
            projectCredits.CurrentPerDay = JsonSerializer.Serialize(costsPerDay);

        // last updated
        projectCredits.LastUpdate = DateTime.UtcNow.Date;
    }    
}
