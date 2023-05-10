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

    public async Task<bool> UpdateProjectUsage(int projectId, string[] resourceGroups, CancellationToken ct)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);

        // find existing 
        var projectCredits = await ctx.Project_Credits.FirstOrDefaultAsync(e => e.ProjectId == projectId, ct);

        // create if it does exists
        projectCredits ??= new() { ProjectId = projectId };

        // update usage
        var (succeeded, yesterdayCosts) = await UpdateUsage(projectCredits, resourceGroups, ct);
        if (!succeeded)
            return false;

        // add or update
        if (projectCredits.Id == 0)
        {
            ctx.Project_Credits.Add(projectCredits);
        }
        else
        {
            ctx.Project_Credits.Update(projectCredits);
        }

        // update the day stats
        if (yesterdayCosts is not null)
            await UpdateYesterdayStats(ctx, projectId, yesterdayCosts);

        // save changes
        await ctx.SaveChangesAsync(ct);

        return true;
    }

    private async Task UpdateYesterdayStats(DatahubProjectDBContext ctx, int projectId, List<AzureServiceCost> yesterdayCosts)
    {
        if (yesterdayCosts.Count == 0)
            return;

        var yesterday = DateTime.Now.AddDays(-1).Date;

        if (await ctx.Project_Costs.AnyAsync(c => c.Project_ID == projectId && c.Date == yesterday))
            return;

        foreach (var serviceCost in yesterdayCosts)
        {
            Datahub_Project_Costs cost = new()
            {
                Project_ID = projectId,
                Date = yesterday,
                CadCost = serviceCost.Cost,
                ServiceName = serviceCost.Name
            };
            ctx.Project_Costs.Add(cost);
        }
    }

    static bool UpdatedInPeriod(DateTime? date, int minutes)
    {
        return date.HasValue && DateTime.UtcNow.AddMinutes(-minutes) < date;
    }

    private async Task<(bool Succeeded, List<AzureServiceCost>? yesterdayCosts)> UpdateUsage(Project_Credits projectCredits, 
        string[] resourceGroups, CancellationToken ct)
    {
        var session = await _azureManagementService.GetSession(ct);

        // monthy by service
        var costByService = await session.GetResourceGroupYearCostByService(resourceGroups);
        if (costByService is not null)
        {
            projectCredits.CurrentPerService = JsonSerializer.Serialize(costByService);
            projectCredits.Current = costByService.Sum(c => c.Cost);
        }
        else
        {
            // allow a grace period for new projects
            if (projectCredits.Id == 0)
                return (false, default);

            // if not updated in the last 90 minutes, throw the exception
            if (!UpdatedInPeriod(projectCredits.LastUpdate, 90))
                throw new Exception($"Failed to read up to date cost for resource group '{resourceGroups}'");
        }

        // wait 5 seconds before trying next query
        await Task.Delay(5000);

        // yesterday by service
        var yesterdayCostByService = await session.GetResourceGroupYesterdayCostByService(resourceGroups);
        if (yesterdayCostByService is not null)
        {
            projectCredits.YesterdayPerService = JsonSerializer.Serialize(yesterdayCostByService);
            projectCredits.YesterdayCredits = yesterdayCostByService.Sum(c => c.Cost);
        }

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // current cost by day
        var costsPerDay = await session.GetResourceGroupMonthlyCostPerDay(resourceGroups);
        if (costsPerDay is not null)
            projectCredits.CurrentPerDay = JsonSerializer.Serialize(costsPerDay);

        // last updated
        projectCredits.LastUpdate = DateTime.UtcNow;

        return (true, yesterdayCostByService);
    }
}

public record DayServiceCosts(DateTime Date, List<AzureServiceCost>? ServiceCosts)
{
    public bool Valid => ServiceCosts is not null;
}
