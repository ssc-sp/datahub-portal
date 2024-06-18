﻿using System.Runtime.CompilerServices;
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
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _config;

        public WorkspaceBudgetManagementService(ArmClient armClient, ILogger<WorkspaceBudgetManagementService> logger,
            IDbContextFactory<DatahubProjectDBContext> dbContextFactory, IConfiguration config)
        {
            _armClient = armClient;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _config = config;
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
            try
            {
                var budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
                return await GetBudgetAmountAsync(budgetId);
            }
            catch (Exception e)
            {
                _logger.LogError("GetWorkspaceBudgetAmountAsync failed", e);
                return 0;
            }
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
        public async Task SetWorkspaceBudgetAmountAsync(string workspaceAcronym, decimal amount, bool rollover = false,
            string? budgetId = null)
        {
            if (budgetId is null)
            {
                try
                {
                    budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
                }
                catch (Exception e)
                {
                    _logger.LogError("SetWorkspaceBudgetAmountAsync failed", e);
                    return;
                }
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
                try
                {
                    budgetId = await GetBudgetIdForWorkspace(workspaceAcronym);
                }
                catch (Exception e)
                {
                    _logger.LogError("GetWorkspaceBudgetSpentAsync failed", e);
                    return 0;
                }
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
            _logger.LogInformation($"Updating budget spent for workspace {workspaceAcronym}");
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
                await ctx.SaveChangesAsync();
            }

            var currentSpent = await GetWorkspaceBudgetSpentAsync(workspaceAcronym, budgetId);
            var beforeUpdateBudgetSpent = (decimal)projectCredits.BudgetCurrentSpent;
            if (currentSpent != 0)
            {
                projectCredits.BudgetCurrentSpent = (double)currentSpent;
            }

            ctx.Project_Credits.Update(projectCredits);
            await ctx.SaveChangesAsync();
            _logger.LogInformation(
                $"Workspace {workspaceAcronym} budget spent updated to {currentSpent} from {beforeUpdateBudgetSpent}");
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
            _logger.LogInformation($"Getting budget id for workspace {workspaceAcronym}");
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            if (rgName is null)
            {
                var projectResources = ctx.Project_Resources2.Include(p => p.Project)
                    .Where(p => p.Project.Project_Acronym_CD == workspaceAcronym).ToList();
                var blobType = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
                var blobResource = projectResources.FirstOrDefault(r => r.ResourceType == blobType);
                if (blobResource is null)
                {
                    _logger.LogInformation(
                        $"Could not find blob storage resource for project with acronym {workspaceAcronym}");
                    rgName = await GetResourceGroupNameFallback(workspaceAcronym);
                }
                else
                {
                    rgName = ParseResourceGroup(blobResource.JsonContent);
                }
            }

            var project = await ctx.Projects.Include(p => p.DatahubAzureSubscription)
                .FirstOrDefaultAsync(p => p.Project_Acronym_CD == workspaceAcronym);
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

            var budgetId =
                $"/subscriptions/{subId}/resourceGroups/{rgName}/providers/Microsoft.Consumption/budgets/{rgName}-budget";
            _logger.LogInformation($"Budget id for workspace {workspaceAcronym} is {budgetId}");
            return budgetId;
        }

        internal async Task<string> GetResourceGroupNameFallback(string workspaceAcronym)
        {
            _logger.LogInformation(
                $"Using Azure Resource Manager to find resource group name for workspace {workspaceAcronym}");
            using var ctx = await _dbContextFactory.CreateDbContextAsync();
            var sub = ctx.Projects.Include(p => p.DatahubAzureSubscription)
                .FirstOrDefault(p => p.Project_Acronym_CD == workspaceAcronym)?.DatahubAzureSubscription;
            var subId = "/subscriptions/" + sub?.SubscriptionId;
            var env = _config["DataHub_ENVNAME"];
            var subResource = _armClient.GetSubscriptionResource(new ResourceIdentifier(subId));
            var rgs = subResource.GetResourceGroups();
            var workspaceRgs = rgs.GetAllAsync(filter: $"tagName eq 'project_cd' and tagValue eq '{workspaceAcronym}'");
            var enumerator = workspaceRgs.GetAsyncEnumerator();
            var rgNames = new List<string>();
            do
            {
                var currentRg = enumerator.Current;
                if (currentRg is null) continue;
                var rgResource = await currentRg.GetAsync();
                if (!rgResource.HasValue) continue;
                var rgName = rgResource.Value.Data.Name;
                if (string.IsNullOrEmpty(rgName)) continue;
                if (!string.IsNullOrEmpty(env)) rgNames = rgNames.Where(rg => rg.Contains(env)).ToList();
                rgNames.Add(rgName);
            } while (await enumerator.MoveNextAsync());

            await enumerator.DisposeAsync();
            var projRgName = rgNames.FirstOrDefault(rg => rg.Contains("_proj_"));
            return projRgName;
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