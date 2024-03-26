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

        public WorkspaceBudgetManagementService(ILogger<WorkspaceBudgetManagementService> logger, ArmClient armClient, IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
        {
            _armClient = armClient;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// Gets the budget amount for the given budget id.
        /// </summary>
        /// <param name="budgetId">The budget identifier</param>
        /// <returns>The budget total amount</returns>
        /// <exception cref="Exception">Throws an exception if the budget could not be found</exception>
        internal async Task<decimal> GetBudgetAmountAsync(string budgetId)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }

            return budget.Data.Amount!.Value;
        }

        /// <summary>
        /// Gets the budget amount for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>The total budget amount</returns>
        public async Task<decimal> GetWorkspaceBudgetAmountAsync(string workspaceAcronym)
        {
            var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            return await GetBudgetAmountAsync(budgetId);
        }

        /// <summary>
        /// Set the budget amount for the given budget id.
        /// </summary>
        /// <param name="budgetId">The budget identifier</param>
        /// <param name="amount">The amount to set the budget to</param>
        /// <exception cref="Exception">Throws an error if the budget could not be found</exception>
        internal async Task SetBudgetAmountAsync(string budgetId, decimal amount)
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
        
        /// <summary>
        /// Sets the budget amount for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="amount">The total budget amount to set</param>
        /// <returns></returns>
        public async Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount, bool rollover = false)
        {
            var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            await SetBudgetAmountAsync(budgetId, amount);
            if (rollover)
            {
                using var ctx = await _dbContextFactory.CreateDbContextAsync();
                var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
                if (project is null)
                {
                    _logger.LogError($"Could not find project with acronym {workspaceAcronym}");
                    throw new Exception($"Could not find project with acronym {workspaceAcronym}");
                }
                var projectCredits = await ctx.Project_Credits
                    .FirstAsync(p => p.ProjectId == project.Project_ID);
                if (projectCredits is null)
                {
                    _logger.LogError($"Could not find project credits for project with acronym {workspaceAcronym}");
                    throw new Exception($"Could not find project credits for project with acronym {workspaceAcronym}");
                }
                projectCredits.LastRollover = DateTime.UtcNow;
                ctx.Project_Credits.Update(projectCredits);
                await ctx.SaveChangesAsync();
            }
        }
        
        internal async Task<decimal> GetBudgetSpentAsync(string budgetId)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                _logger.LogError($"Could not parse data from budget with id {budgetId}");
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }
            return budget.Data.CurrentSpend.Amount!.Value;
        }
        
        /// <summary>
        /// Get the amount of budget spent for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>The amount of budget spent</returns>
        public async Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym)
        {
            var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            return await GetBudgetSpentAsync(budgetId);
        }

        /// <summary>
        /// Updates the amount of budget spent saved in the Project_Credits table for a given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>A tuple of whether or not the budget spent has decreased (indicating a budget reset), and the budget spent before the reset</returns>
        public async Task<(bool, decimal)> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym)
        {
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

        /// <summary>
        /// Gets the azure budget resource for the given budget id.
        /// </summary>
        /// <param name="budgetId">The budget identifier</param>
        /// <returns>The consumption budget resource</returns>
        /// <exception cref="Exception">Throws an exception if the budget with given id does not exist or if it could not be read</exception>
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

        /// <summary>
        /// Gets the budget id for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <returns>The budget id</returns>
        internal async Task<string> GetBudgetIdForWorkspace(string workspaceAcronym)
        {
            
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var projectResources = ctx.Project_Resources2.Include(p => p.Project)
                .Where(p => p.Project.Project_Acronym_CD == workspaceAcronym).ToList();
            var blobType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
            var blobResource = projectResources.First(r => r.ResourceType == blobType);
            var resourceGroupName = ParseResourceGroup(blobResource.JsonContent);
            var budgetId =
                $"/subscription/{_armClient.GetDefaultSubscription().Id.SubscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.Consumption/budgets/{resourceGroupName}-budget";
            return budgetId;
        }

        /// <summary>
        /// Gets the resource group name from the json content of the blob storage resource
        /// </summary>
        /// <param name="jsonContent">Project_Resource.JsonContent of the blob storage resource of a workspace</param>
        /// <returns>The resource group name of the workspace</returns>
        internal string ParseResourceGroup(string jsonContent)
        {
            var content = JsonSerializer.Deserialize<RgNameObject>(jsonContent);
            return content?.resource_group_name;
        }
        internal record RgNameObject(string resource_group_name);
    }
}