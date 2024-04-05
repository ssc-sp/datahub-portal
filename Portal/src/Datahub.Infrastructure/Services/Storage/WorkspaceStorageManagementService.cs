using System.Text.Json;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using Azure.ResourceManager.Storage;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Storage
{
    public class WorkspaceStorageManagementService : IWorkspaceStorageManagementService
    {
        private readonly ArmClient _armClient;
        private readonly ILogger<WorkspaceStorageManagementService> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public WorkspaceStorageManagementService(ArmClient armClient, ILogger<WorkspaceStorageManagementService> logger, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _armClient = armClient;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<double> GetStorageCapacity(string workspaceAcronym, List<string>? storageAccountIds = null)
        {
            if (storageAccountIds is null)
            {
                storageAccountIds = await GetStorageAccountIds(workspaceAcronym);
            }

            var today = DateTime.UtcNow;
            var dateFormat = "yyyy-MM-ddTHH:00:00.000Z";
            var fromDate = today.AddDays(-1).ToString(dateFormat);
            var toDate = today.ToString(dateFormat);
            
            var totalCapacity = 0.0;

            storageAccountIds.ForEach(id =>
            {
                var storageId = new ResourceIdentifier(id);
                var option = new ArmResourceGetMonitorMetricsOptions();
                option.Metricnames = "UsedCapacity";
                option.Aggregation = "average";
                option.Timespan = $"{fromDate}/{toDate}";
                option.Metricnamespace = "Microsoft.Storage/storageAccounts";
                option.ValidateDimensions = false;
                var metrics = _armClient.GetMonitorMetrics(storageId, option).ToList();
                var timeseries = metrics[0].Timeseries.ToList()[0];
                var timeseriesData = timeseries.Data.ToList().Last();
                var average = timeseriesData.Average;
                if (average is null)
                {
                    _logger.LogError($"Could not parse data from storage account with id {id}");
                    throw new Exception($"Could not parse data from storage account with id {id}");
                }

                totalCapacity += average!.Value;
            });
            
            return totalCapacity;
        }

        public async Task<double> UpdateStorageCapacity(string workspaceAcronym, List<string>? storageAccountIds = null)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var date = DateTime.UtcNow.Date;
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            if (project is null)
            {
                _logger.LogError($"Project with acronym {workspaceAcronym} not found");
                throw new Exception($"Project with acronym {workspaceAcronym} not found");
            }
            
            var projectAverage = await ctx.Project_Storage_Avgs.FirstOrDefaultAsync(p => p.ProjectId == project.Project_ID && p.Date == date);
            if (projectAverage is null)
            {
                projectAverage = new Project_Storage { ProjectId = project.Project_ID };
                ctx.Project_Storage_Avgs.Add(projectAverage);
                await ctx.SaveChangesAsync();
            }
            
            var capacity = await GetStorageCapacity(workspaceAcronym, storageAccountIds);
            projectAverage.AverageCapacity = capacity;
            projectAverage.Date = date;
            projectAverage.CloudProvider = "azure";
            ctx.Project_Storage_Avgs.Update(projectAverage);
            await ctx.SaveChangesAsync();
            return capacity;
        }

        internal async Task<StorageAccountResource> GetAzureStorageAccountResource(string storageId)
        {
            var id = new ResourceIdentifier(storageId);
            var storageAccount = _armClient.GetStorageAccountResource(id);
            if (storageAccount == null)
            {
                _logger.LogError($"Storage account with id {storageId} not found");
                throw new Exception($"Storage account with id {storageId} not found");
            }

            var response = await storageAccount.GetAsync();

            if (response.Value == null)
            {
                _logger.LogError($"Storage account with id {storageId} not found");
                throw new Exception($"Storage account with id {storageId} not found");
            }

            return response.Value;
        }

        internal async Task<List<string>> GetStorageAccountIds(string workspaceAcronym)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var projectResources = ctx.Project_Resources2.Include(p => p.Project)
                .Where(p => p.Project.Project_Acronym_CD == workspaceAcronym).ToList();

            var rgNames = await GetResourceGroupNames(workspaceAcronym);
            var storageIds = new List<string>();
            
            rgNames.ForEach(async rg =>
            {
                var rgId = $"/subscriptions/{_armClient.GetDefaultSubscription().Id.SubscriptionId}/resourceGroups/{rg}";
                var resourceId = new ResourceIdentifier(rgId);
                var resourceGroup = _armClient.GetResourceGroupResource(resourceId);
                if (resourceGroup == null)
                {
                    _logger.LogError($"Resource group with id {rgId} not found");
                    throw new Exception($"Resource group with id {rgId} not found");
                }
                var response = await resourceGroup.GetAsync();
                if (response.Value == null)
                {
                    _logger.LogError($"Resource group with id {rgId} not found");
                    throw new Exception($"Resource group with id {rgId} not found");
                }

                var storageAccounts = response.Value.GetStorageAccounts().ToList();
                
                storageAccounts.ForEach(sa =>
                {
                    storageIds.Add(sa.Id);
                });
            });
            
            return storageIds;
        }
        
        internal async Task<List<string>> GetResourceGroupNames(string workspaceAcronym)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();

            var blobStorageType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
            var dbkType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);

            var resources = ctx.Project_Resources2.Include(r => r.Project)
                .Where(r => r.Project.Project_Acronym_CD == workspaceAcronym);

            var rgNames = new List<string>();
            var dbkIds = new HashSet<int>();

            await resources.ForEachAsync(r =>
            {
                if (r.ResourceType == dbkType)
                {
                    dbkIds.Add(r.ProjectId);
                }
            });

            await resources.ForEachAsync(r =>
            {
                if (r.ResourceType == blobStorageType)
                {
                    rgNames.Add(ParseResourceGroup(r.JsonContent));
                    if (dbkIds.Contains(r.ProjectId))
                    {
                        rgNames.Add(ParseDbkResourceGroup(r.JsonContent));
                        dbkIds.Remove(r.ProjectId);
                    }
                }
            });

            return rgNames;
        }

        internal string ParseResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            return content?.resource_group_name;
        }
        
        internal string ParseDbkResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            var dbk = content.resource_group_name.Split("_").Select((s, idx) => idx == 1 ? "dbk" : s);
            var dbkRg = string.Join("-", dbk);
            return dbkRg;
        }
        internal record RgNameObject(string resource_group_name);
    }
}