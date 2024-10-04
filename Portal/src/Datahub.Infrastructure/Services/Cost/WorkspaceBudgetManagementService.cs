using System.Runtime.CompilerServices;
using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Consumption;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Core.Model.Projects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Datahub.Core.Model.Context;

[assembly: InternalsVisibleTo("Datahub.SpecflowTests")]

namespace Datahub.Infrastructure.Services.Cost
{
    public class WorkspaceBudgetManagementService(
        ArmClient armClient,
        ILogger<WorkspaceBudgetManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IWorkspaceResourceGroupsManagementService workspaceRgMgmtService)
        : IWorkspaceBudgetManagementService
    {
        public const string PROJECT_RG_KEY = "_proj_";
        
        #region Implementation

        /// <inheritdoc />
        public async Task<decimal> GetWorkspaceBudgetAmountAsync(string workspaceAcronym, List<string>? budgetIds = null)
        {
            try
            {
                if (budgetIds is null)
                {
                    budgetIds = await GetBudgetIdForWorkspace(workspaceAcronym);
                }
                return await GetBudgetAmountAsync(budgetIds.First());
            }
            catch (Exception e)
            {
                logger.LogInformation("Could not get budget ID for workspace {WorkspaceAcronym}", workspaceAcronym);
                throw new Exception($"Could not get budget ID for workspace {workspaceAcronym}: {e.Message}");
            }
        }

        /// <inheritdoc />
        public async Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount, bool rollover = false,
            List<string>? budgetIds = null)
        {
            if (budgetIds is null)
            {
                try
                {
                    budgetIds = await GetBudgetIdForWorkspace(workspaceAcronym);
                }
                catch (Exception e)
                {
                    logger.LogError("Could not get budget ID for workspace {WorkspaceAcronym}", workspaceAcronym);
                    throw new Exception($"Could not get budget ID for workspace {workspaceAcronym}: {e.Message}");
                }
            }

            foreach (var budgetId in budgetIds)
            {
                await SetBudgetAmountAsync(budgetId, amount);
            }

            if (rollover)
            {
                using var ctx = await dbContextFactory.CreateDbContextAsync();
                var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == workspaceAcronym);

                var projectCredits = await ctx.Project_Credits
                    .FirstAsync(p => p.ProjectId == project.Project_ID);

                projectCredits.LastRollover = DateTime.UtcNow;
                ctx.Project_Credits.Update(projectCredits);
                await ctx.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public async Task<decimal> GetWorkspaceBudgetSpentAsync(string workspaceAcronym, List<string>? budgetIds = null)
        {
            if (budgetIds is null)
            {
                try
                {
                    budgetIds = await GetBudgetIdForWorkspace(workspaceAcronym);
                }
                catch (Exception e)
                {
                    logger.LogInformation("Could not get budget ID for workspace {WorkspaceAcronym}", workspaceAcronym);
                    throw new Exception($"Could not get budget ID for workspace {workspaceAcronym}: {e.Message}");
                }
            }

            decimal total = 0;
            foreach (var budgetId in budgetIds)
            {
                total += await GetBudgetSpentAsync(budgetId);
            }

            return total;
        }

        /// <inheritdoc />
        public async Task<decimal> UpdateWorkspaceBudgetSpentAsync(string workspaceAcronym,
            List<string>? budgetIds = null)
        {
            logger.LogInformation("Updating budget spent for workspace {WorkspaceAcronym}", workspaceAcronym);
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.FirstAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            var projectCredits = await ctx.Project_Credits
                .FirstOrDefaultAsync(p => p.ProjectId == project.Project_ID);

            if (projectCredits is null)
            {
                projectCredits = new Project_Credits()
                {
                    ProjectId = project.Project_ID
                };
                ctx.Project_Credits.Add(projectCredits);
                await ctx.SaveChangesAsync();
            }

            var currentSpent = await GetWorkspaceBudgetSpentAsync(workspaceAcronym, budgetIds);
            logger.LogInformation("Current spent for workspace {WorkspaceAcronym} is {CurrentSpent}", workspaceAcronym,
                currentSpent);
            var beforeUpdateBudgetSpent = (decimal)projectCredits.BudgetCurrentSpent;
            if (currentSpent > (decimal)0.0)
            {
                projectCredits.BudgetCurrentSpent = (double)currentSpent;
            }

            ctx.Project_Credits.Update(projectCredits);
            await ctx.SaveChangesAsync();
            logger.LogInformation(
                "Workspace {WorkspaceAcronym} budget spent updated to {CurrentSpent} from {BeforeUpdateBudgetSpent}",
                workspaceAcronym, currentSpent, beforeUpdateBudgetSpent);
            return beforeUpdateBudgetSpent;
        }

        #endregion

        #region Internal Methods

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
                logger.LogError("Could not parse data from budget with id {BudgetId}", budgetId);
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }

            return budget.Data.Amount!.Value;
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
                logger.LogError("Could not parse data from budget with id {BudgetId}", budgetId);
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }

            budget.Data.Amount = amount;
            await budget.UpdateAsync(WaitUntil.Completed, budget.Data);
        }

        /// <summary>
        /// Gets the budget spent for the given budget id.
        /// </summary>
        /// <param name="budgetId">The Azure ID of the budget</param>
        /// <returns>The amount spent for that budget</returns>
        /// <exception cref="Exception">Throws if it cannot find the budget</exception>
        internal async Task<decimal> GetBudgetSpentAsync(string budgetId)
        {
            var budget = await GetBudgetAzureResource(budgetId);
            if (!budget.HasData)
            {
                logger.LogError("Could not parse data from budget with id {BudgetId}", budgetId);
                throw new Exception($"Could not parse data from budget with id {budgetId}");
            }

            return budget.Data.CurrentSpend.Amount!.Value;
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
            var budget = armClient.GetConsumptionBudgetResource(budgetIdentifier);
            if (budget == null)
            {
                logger.LogError("Budget with id {BudgetId} not found", budgetId);
                throw new Exception($"Budget with id {budgetId} not found");
            }

            var response = await budget.GetAsync();

            if (response.Value == null)
            {
                logger.LogError("Budget with id {BudgetId} not found", budgetId);
                throw new Exception($"Budget with id {budgetId} not found");
            }

            return response.Value;
        }

        /// <summary>
        /// Gets the budget id for the given workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym</param>
        /// <param name="rgNames">Optional resource group names to use to get budget id</param>
        /// <returns>The budget id</returns>
        internal async Task<List<string>> GetBudgetIdForWorkspace(string workspaceAcronym, List<string>? rgNames = null)
        {
            logger.LogInformation("Getting budget id for workspace {WorkspaceAcronym}", workspaceAcronym);
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            if (rgNames is null)
            {
                var allRgNames = await workspaceRgMgmtService.GetWorkspaceResourceGroupsAsync(workspaceAcronym);
                var projRgName = allRgNames.FirstOrDefault(rg => rg.Contains(PROJECT_RG_KEY));
                if (projRgName is null)
                {
                    logger.LogError("Could not find project resource group for workspace {WorkspaceAcronym}", workspaceAcronym);
                    throw new Exception($"Could not find project resource group for workspace {workspaceAcronym}");
                }
                rgNames = new List<string> {projRgName};
            }

            var project = await ctx.Projects.Include(p => p.DatahubAzureSubscription)
                .FirstAsync(p => p.Project_Acronym_CD == workspaceAcronym);
            
            var subId = "/subscriptions/" + project.DatahubAzureSubscription?.SubscriptionId;

            var budgetIds = rgNames.Select(rg => GetBudgetId(subId, rg)).ToList();
            logger.LogInformation("Budget ids for workspace {WorkspaceAcronym} is {Join}", workspaceAcronym, string.Join(", ", budgetIds));
            return budgetIds;
        }

        /// <summary>
        /// Gets the budget ID for the given subscription ID and resource group name.
        /// </summary>
        /// <param name="subId">Subscription ID</param>
        /// <param name="rgName">Resource group name</param>
        /// <returns>A string representing the Azure ID of the budget</returns>
        internal string GetBudgetId(string subId, string rgName)
        {
            var budgetId =
                $"/subscriptions/{subId}/resourceGroups/{rgName}/providers/Microsoft.Consumption/budgets/{rgName}-budget";
            return budgetId;
        }
        #endregion
    }
}