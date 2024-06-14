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
[assembly: InternalsVisibleTo("Datahub.SpecflowTests")]

namespace Datahub.Infrastructure.Services.Cost
{
    public class WorkspaceBudgetManagementService : IWorkspaceBudgetManagementService
    {
        private ArmClient _armClient;
        private readonly ILogger<WorkspaceBudgetManagementService> _logger;
        private readonly IDbContextFactory<DatahubProjectDBContext> _dbContextFactory;

        public WorkspaceBudgetManagementService(ArmClient armClient, ILogger<WorkspaceBudgetManagementService> logger,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory)
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
        /// <param name="rollover">If the operation is part of a rollover. This will update the credits to keep track of rollovers</param>
        /// <param name="budgetId">Optional budget id to use. If not provided, will interpolate</param>
        /// <returns></returns>
        public async Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount, bool rollover = false, string? budgetId = null)
        {
            if (budgetId is null)
            {
                budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            }
            
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
        /// <param name="budgetId">Optional budget id to use, if not provided, will interpolate id</param>
        /// <returns>The amount of budget spent</returns>
        public async Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym, string? budgetId = null)
        {
            if (budgetId is null)
            {
                budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
            }

            return await GetBudgetSpentAsync(budgetId);
        }

        /// <summary>
        /// Updates the amount of budget spent saved in the Project_Credits table for a given workspace acronym.
        /// 
        /// A budget rollover is true when the spent budget goes down, which can only be triggered when a budget is reset.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="budgetId">Optional budget id to use. If not provided, will interpolate the id.</param>
        /// <returns>A tuple of whether or not the budget spent has decreased (indicating a budget reset), and the budget spent before the reset</returns>
        public async Task<decimal> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym,
            string? budgetId = null)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            var projectCredits = await ctx.Project_Credits
                .FirstOrDefaultAsync(p => p.ProjectId == project.Project_ID);

            if (projectCredits is null)
            {
                projectCredits = new Project_Credits()
                {
                    ProjectId = project.Project_ID
                };
                ctx.Project_Credits.Add(projectCredits);
            }

            var currentSpent = await GetWorkspaceBudgetSpentAsync(workspaceAcronym, budgetId);
            var beforeUpdateBudgetSpent = (decimal)projectCredits.BudgetCurrentSpent;
            projectCredits.BudgetCurrentSpent = (double)currentSpent;

            ctx.Project_Credits.Update(projectCredits);
            await ctx.SaveChangesAsync();
            return beforeUpdateBudgetSpent;
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
        /// <param name="rgName">Optional resource group name to use to get budget id</param>
        /// <returns>The budget id</returns>
        internal async Task<string> GetBudgetIdForWorkspace(string workspaceAcronym, string? rgName = null)
        {
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            if (rgName is null)
            {
                var projectResources = ctx.Project_Resources2.Include(p => p.Project)
                    .Where(p => p.Project.Project_Acronym_CD == workspaceAcronym).ToList();
                var blobType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
                var blobResource = projectResources.First(r => r.ResourceType == blobType);
                rgName = ParseResourceGroup(blobResource.JsonContent);
            }
            var project= await ctx.Projects.Include(p => p.DatahubAzureSubscription).FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            if (project is null)
            {
                _logger.LogError($"Could not find project with acronym {workspaceAcronym}");
                throw new Exception($"Could not find project with acronym {workspaceAcronym}");
            }

            var subId = project.DatahubAzureSubscription.SubscriptionId;
            if (subId is null)
            {
                _logger.LogError($"Could not find subscription id for project with acronym {workspaceAcronym}");
                throw new Exception($"Could not find subscription id for project with acronym {workspaceAcronym}");
            }

            var budgetId = $"/subscription/{subId}/resourceGroups/{rgName}/providers/Microsoft.Consumption/budgets/{rgName}-budget";
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