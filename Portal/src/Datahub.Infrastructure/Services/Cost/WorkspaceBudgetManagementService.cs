using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Datahub.Application.Services.Budget;
using Azure.ResourceManager.Consumption;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Datahub.Infrastructure.UnitTests")]

namespace Datahub.Infrastructure.Services.Cost
{
    public class WorkspaceBudgetManagementService : IWorkspaceBudgetManagementService
    {
        private ArmClient _armClient;
        private readonly ILogger<WorkspaceBudgetManagementService> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;
        public string SubscriptionId { get; set; }

        public WorkspaceBudgetManagementService(ILogger<WorkspaceBudgetManagementService> logger, ArmClient armClient, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _armClient = armClient;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<decimal> GetBudgetAmountAsync(string budgetId)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }

            return budget.Data.Amount!.Value;
        }

        public async Task<decimal> GetWorkspaceBudgetAmountAsync(string workspaceAcronym)
        {
            var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            return await GetBudgetAmountAsync(budgetId);
        }

        public async Task SetBudgetAmountAsync(string budgetId, decimal amount)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }
            
            budget.Data.Amount = amount;
            await budget.UpdateAsync(WaitUntil.Completed, budget.Data);
        }
        
        public async Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount)
        {
            var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            await SetBudgetAmountAsync(budgetId, amount);
        }
        
        public async Task<decimal> GetBudgetSpentAsync(string budgetId)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }
            return budget.Data.CurrentSpend.Amount!.Value;
        }
        
        public async Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym)
        {
            var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            return await GetBudgetSpentAsync(budgetId);
        }

        public async Task<(bool, decimal)> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym)
        {
            var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            var projectCredits = await ctx.Project_Credits
                .FirstAsync(p => p.ProjectId == project.Project_ID);
            
            if (projectCredits is null)
            {
                projectCredits = new Project_Credits()
                {
                    ProjectId = project.Project_ID
                };
                ctx.Project_Credits.Add(projectCredits);
            }

            var currentSpent = await GetWorkspaceBudgetSpentAsync(workspaceAcronym);
            var beforeUpdateBudgetSpent = (decimal)projectCredits.BudgetCurrentSpent;
            projectCredits.BudgetCurrentSpent = (double)currentSpent;

            ctx.Project_Credits.Update(projectCredits);
            await ctx.SaveChangesAsync();
            return (currentSpent < beforeUpdateBudgetSpent, beforeUpdateBudgetSpent);
        }

        internal async Task<ConsumptionBudgetResource> GetBudgetAzureResource(string budgetId)
        {
            var budgetIdentifier = new ResourceIdentifier(budgetId);
            var budget = _armClient.GetConsumptionBudgetResource(budgetIdentifier);
            if (budget == null)
            {
                _logger.LogError($"Budget with id {budgetId} not found");
                throw new Exception($"Budget with id {budgetId} not found");
            }
            
            var response = await budget.GetAsync();

            if (response.Value == null)
            {
                _logger.LogError($"Budget with id {budgetId} not found");
                throw new Exception($"Budget with id {budgetId} not found");
            }

            return response.Value;
        }

        internal async Task<string> GetBudgetIdForWorkspace(string workspaceAcronym)
        {
            if (SubscriptionId is null)
            {
                _logger.LogError("SubscriptionId must be set to determine budget ids for workspaces.");
                throw new Exception("SubscriptionId must be set to determine budget ids for workspaces.");
            }
            
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var projectResources = ctx.Project_Resources2.Include(p => p.Project)
                .Where(p => p.Project.Project_Acronym_CD == workspaceAcronym).ToList();
            var blobType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
            var blobResource = projectResources.First(r => r.ResourceType == blobType);
            var resourceGroupName = ParseResourceGroup(blobResource.JsonContent);
            var budgetId =
                $"/subscription/{SubscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Consumption/budgets/{resourceGroupName}-budget";
            return budgetId;
        }

        internal string ParseResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            return content?.resource_group_name;
        }
        internal record RgNameObject(string resource_group_name);
    }
}