using AngleSharp.Common;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Azure;
using Lucene.Net.QueryParsers.Classic;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Graph;

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

    public async Task<bool> UpdateProjectUsage(int projectId, string resourceGroup, CancellationToken ct)
    {
        using var ctx = await _dbContextFactory.CreateDbContextAsync(ct);

        // find existing 
        var projectCredits = await ctx.Project_Credits.FirstOrDefaultAsync(e => e.ProjectId == projectId, ct);

        // create if it does exists
        projectCredits ??= new() { ProjectId = projectId };

        // update usage
        if (!await UpdateUsage(projectCredits, resourceGroup, ct))
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

        // save changes
        await ctx.SaveChangesAsync(ct);

        return true;
    }

    static bool UpdatedInPeriod(DateTime? date, int minutes)
    {
        return date.HasValue && DateTime.UtcNow.AddMinutes(-minutes) < date;
    }

    private async Task<bool> UpdateUsage(Project_Credits projectCredits, string resourceGroup, CancellationToken ct)
    {
        var session = await _azureManagementService.GetSession(ct);

        // get main resource group stats
        var statsMain = await GetResourceGroupStats(session, resourceGroup, ct);

        // wait 5 seconds before trying next resource group
        await Task.Delay(5000);

        // get databricks resource group stats
        var statsDbrk = await GetResourceGroupStats(session, GetDatabricksResourceGroup(resourceGroup), ct);

        // add the stats
        var stats = statsMain + statsDbrk;

        // current monthly cost
        var yearCost = stats.Year;
        if (yearCost is null)
        {
            // allow a grace period for new projects
            if (projectCredits.Id == 0)
                return false;

            // if not updated in the last 90 minutes, throw the exception
            if (!UpdatedInPeriod(projectCredits.LastUpdate, 90))
                throw new Exception($"Failed to read up to date cost for resource group '{resourceGroup}'");
        }
        else
        {
            projectCredits.Current = yearCost.Value;
        }       

        // monthy by service
        var costByService = stats.YearPerService;
        if (costByService is not null)
            projectCredits.CurrentPerService = JsonSerializer.Serialize(costByService);

        // yesterday cost
        var yesterdayCredits = stats.Yesterday;
        if (yesterdayCredits.HasValue)
            projectCredits.YesterdayCredits = yesterdayCredits.Value;

        // yesterday by service
        var yesterdayCostByService = stats.YesterdayPerService;
        if (yesterdayCostByService is not null)
            projectCredits.YesterdayPerService = JsonSerializer.Serialize(yesterdayCostByService);

        // current cost by day
        var costsPerDay = stats.CostPerDay;
        if (costsPerDay is not null)
            projectCredits.CurrentPerDay = JsonSerializer.Serialize(costsPerDay);

        // last updated
        projectCredits.LastUpdate = DateTime.UtcNow;

        return true;
    }
    
    private async Task<StatsVector> GetResourceGroupStats(AzureManagementSession session, string resourceGroup, CancellationToken ct)
    {
        // year cost
        var yearCost = await session.GetResourceGroupLastYearCost(resourceGroup);

        // wait 2 seconds before trying next query
        await Task.Delay(5000);

        // monthy by service
        var costByService = await session.GetResourceGroupYearCostByService(resourceGroup);

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // yesterday cost
        var yesterdayCredits = await session.GetResourceGroupYesterdayCost(resourceGroup);

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // yesterday by service
        var yesterdayCostByService = await session.GetResourceGroupYesterdayCostByService(resourceGroup);

        // wait 2 seconds before trying next query
        await Task.Delay(2000);

        // current cost by day
        var costsPerDay = await session.GetResourceGroupMonthlyCostPerDay(resourceGroup);

        return new(yearCost, costByService, yesterdayCredits, yesterdayCostByService, costsPerDay);
    }

    static string GetDatabricksResourceGroup(string name)
    {
        // fsdh_proj_die1_dev_rg => fsdh-dbk-die1-dev-rg
        var parts = name.Split('_').Select((s, idx) => idx == 1 ? "dbk" : s);
        return string.Join("-", parts);
    }
}

record StatsVector(double? Year, List<AzureServiceCost>? YearPerService, double? Yesterday, List<AzureServiceCost>? YesterdayPerService, List<AzureDailyCost>? CostPerDay)
{
    public static StatsVector operator +(StatsVector v1, StatsVector v2)
    {
        var year = AddCosts(v1.Year, v2.Year);
        var yearPerService = Concat(v1.YearPerService, v2.YearPerService);
        var yesterday = AddCosts(v1.Yesterday, v2.Yesterday);
        var yesterdayPerService = Concat(v1.YesterdayPerService, v2.YesterdayPerService);
        var costPerDay = Mix(v1.CostPerDay, v2.CostPerDay);
        return new StatsVector(year, yearPerService, yesterday, yesterdayPerService, costPerDay);
    }

    static double? AddCosts(double? c1, double? c2)
    {
        return c1 is null || c2 is null ? default : c1.Value + c2.Value;
    }

    static List<AzureServiceCost>? Concat(List<AzureServiceCost>? l1, List<AzureServiceCost>? l2)
    {
        if (l1 is null || l2 is null)
            return null;

        var result = new List<AzureServiceCost>(l1);
        result.AddRange(l2.Select(e => new AzureServiceCost() 
        { 
            Name = $"[Databricks Managed] {e.Name}", 
            Cost = e.Cost 
        }));

        return result;
    }

    static List<AzureDailyCost>? Mix(List<AzureDailyCost>? l1, List<AzureDailyCost>? l2)
    {
        if (l1 is null || l2 is null)
            return default;

        if (l1.Count != l2.Count)
            return default;

        return l1.Zip(l2).Select(v => new AzureDailyCost() 
        { 
            Date = v.First.Date, 
            Cost = v.First.Cost + v.Second.Cost 
        }).ToList();
    }
}
