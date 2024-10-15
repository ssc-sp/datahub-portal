using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Core.Extensions;
using Datahub.Core.Model.Context;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.ResourceGroups
{
    public class WorkspaceResourceGroupsManagementService(
        ArmClient armClient,
        ILogger<WorkspaceResourceGroupsManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        IConfiguration config) : IWorkspaceResourceGroupsManagementService
    {
        private const string RESOURCE_GROUP_KEY = "resourceGroups";
        private const string SUBSCRIPTION_KEY = "subscriptions";
        private const string FSDH_RG_PREFIX = "fsdh";
        private const string DBK_RG_KEY = "dbk";
        private const string PROJ_RG_KEY = "proj";


        #region Implementations

        /// <inheritdoc />
        public async Task<List<string>> GetWorkspaceResourceGroupsAsync(string workspaceAcronym)
        {
            logger.LogInformation("Getting resource groups for workspace {WorkspaceAcronym}", workspaceAcronym);
            logger.LogInformation("Attempting from database first");
            var rgs = await GetWorkspaceResourceGroupsFromDbAsync(workspaceAcronym);
            if (rgs.Count == 0)
            {
                logger.LogWarning("No resource groups found in database for {WorkspaceAcronym}, attempting from ARM",
                    workspaceAcronym);
                rgs = await GetWorkspaceResourceGroupsFromArmAsync(workspaceAcronym);
            }

            logger.LogInformation("{Count} resource groups for {WorkspaceAcronym}: {ResourceGroups}", rgs.Count,
                workspaceAcronym,
                string.Join(", ", rgs));
            return rgs;
        }


        /// <inheritdoc />
        public async Task<List<ResourceIdentifier>> GetWorkspaceResourceGroupsIdentifiersAsync(string workspaceAcronym)
        {
            logger.LogInformation("Getting resource groups identifiers for workspace {WorkspaceAcronym}",
                workspaceAcronym);
            logger.LogInformation("Attempting from database first");
            var rgs = await GetWorkspaceResourceGroupsFromDbAsync(workspaceAcronym);
            var sub = await GetWorkspaceSubscriptionId(workspaceAcronym);
            var identifiers = rgs.Select(rg => ToResourceGroupIdentifier(sub,
                rg)).ToList();
            if (identifiers.Count == 0)
            {
                logger.LogWarning(
                    "No resource groups identifiers found in database for {WorkspaceAcronym}, attempting from ARM",
                    workspaceAcronym);
                identifiers = await GetWorkspaceResourceGroupsIdentifiersFromArmAsync(workspaceAcronym);
            }

            logger.LogInformation("{Count} resource groups identifiers for {WorkspaceAcronym}: {Identifiers}",
                identifiers.Count, workspaceAcronym,
                string.Join(", ", identifiers));
            return identifiers;
        }

        /// <inheritdoc />
        public async Task<List<string>> GetAllSubscriptionResourceGroupsAsync(string subscriptionId)
        {
            logger.LogInformation("Getting all resource groups from ARM for subscription {SubId}", subscriptionId);
            var rgs = await GetAllSubscriptionResourceGroupsResourceFromArmAsync(subscriptionId);
            return rgs.Select(rg => rg.Data.Name).ToList();
        }

        /// <inheritdoc />
        public async Task<List<string>> GetAllResourceGroupsAsync()
        {
            logger.LogInformation("Getting all resource groups");
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var projects = ctx.Projects.Include(p => p.DatahubAzureSubscription).ToList();
            var rgNames = new List<string>();
            foreach (var project in projects)
            {
                rgNames.AddRange(await GetWorkspaceResourceGroupsAsync(project.Project_Acronym_CD));
            }

            logger.LogInformation("{Count} total resource groups: {ResourceGroups}", rgNames.Count,
                string.Join(", ", rgNames));
            return rgNames;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Queries the database for all resource groups associated with a workspace. This will look for
        /// blob storage resources and databricks resources and return the resource group names based on the value
        /// stored in the blob storage resource's JsonContent field. This will interpolate the databricks resource group
        /// name from the project resource group name if a databricks resource is found.
        /// </summary>
        /// <param name="workspaceAcronym"></param>
        /// <returns></returns>
        internal async Task<List<string>> GetWorkspaceResourceGroupsFromDbAsync(string workspaceAcronym)
        {
            // Arrange
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = await ctx.Projects.Include(p => p.Resources)
                .FirstAsync(p => p.Project_Acronym_CD == workspaceAcronym);

            var rgNames = new List<string>();

            try
            {
                var rgName = project.GetResourceGroupName();
                rgNames.Add(rgName);
            }
            catch
            {
                logger.LogWarning(
                    "Resource group name not found in new-project-template for {WorkspaceAcronym}. Attempting from blob",
                    workspaceAcronym);
                try
                {
                    var rgName = project.GetResourceGroupNameFromBlob();
                    rgNames.Add(rgName);
                }
                catch
                {
                    logger.LogWarning("Resource group name not found in blob storage for {WorkspaceAcronym}",
                        workspaceAcronym);
                }
            }

            var projRgName = rgNames.FirstOrDefault(rg => rg.Contains($"_{PROJ_RG_KEY}_{workspaceAcronym}_", StringComparison.InvariantCultureIgnoreCase));
            if (projRgName != null)
            {
                try
                {
                    var dbkRgName = project.GetDbkResourceGroupName(projRgName);
                    rgNames.Add(dbkRgName);
                }
                catch
                {
                    logger.LogInformation("No databricks resource found for {WorkspaceAcronym}", workspaceAcronym);
                }
            }

            return rgNames;
        }

        /// <summary>
        /// Converts a subscription id and a resource group name to a ResourceIdentifier
        /// </summary>
        /// <param name="subscriptionId">The sub id the workspace is in</param>
        /// <param name="resourceGroupName">The resource group name to use</param>
        /// <returns>A ResourceIdentifier representing the resource group given</returns>
        internal ResourceIdentifier ToResourceGroupIdentifier(string subscriptionId, string resourceGroupName)
        {
            return new ResourceIdentifier(
                $"/{SUBSCRIPTION_KEY}/{subscriptionId}/{RESOURCE_GROUP_KEY}/{resourceGroupName}");
        }

        /// <summary>
        /// Queries the Azure Resource Manager API for all workspaces in a sub that have a tag that contains the workspace acronym
        /// and that contain the environment name in the resource group name
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to use</param>
        /// <returns>A list of the names of the resource groups that belong to this workspace</returns>
        internal async Task<List<string>> GetWorkspaceResourceGroupsFromArmAsync(string workspaceAcronym)
        {
            var identifiers = await GetWorkspaceResourceGroupsIdentifiersFromArmAsync(workspaceAcronym);
            var rgs = identifiers.Select(id => id.ResourceGroupName!).ToList();
            return rgs;
        }

        /// <summary>
        /// Queries the Azure Resource Manager API for all workspaces in a sub that have a tag that contains the workspace acronym
        /// and that contain the environment name in the resource group name
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to use</param>
        /// <returns>A list of ResourceIdentifier representing all the Azure identifiers for the resource groups
        /// belonging to the workspace given</returns>
        internal async Task<List<ResourceIdentifier>> GetWorkspaceResourceGroupsIdentifiersFromArmAsync(
            string workspaceAcronym)
        {
            var sub = await GetWorkspaceSubscriptionId(workspaceAcronym);
            var rgs = await GetAllSubscriptionResourceGroupsResourceFromArmAsync(sub, workspaceAcronym);
            return rgs.Select(r => r.Id).ToList();
        }

        /// <summary>
        /// Queries the Azure Resource Manager API for all resource groups in a subscription that contain the environment name.
        /// If a workspace acronym is given, will filter only resource groups that contain the workspace acronym in the tag.
        /// </summary>
        /// <param name="subscriptionId">The subscription id to look into</param>
        /// <param name="workspaceAcronym">Optional workspace acronym to filter for specific workspaces</param>
        /// <returns>A list of ResourceIdentifiers representing the resource groups in the subscription/workspace</returns>
        /// <exception cref="Exception">Throws an exception if we cant determine the environment</exception>
        internal async Task<List<ResourceGroupResource>> GetAllSubscriptionResourceGroupsResourceFromArmAsync(
            string subscriptionId, string? workspaceAcronym = null)
        {
            logger.LogInformation("Getting all resource groups from ARM for subscription {SubId}", subscriptionId);
            var subId = subscriptionId.Contains("/") ? subscriptionId : $"/{SUBSCRIPTION_KEY}/{subscriptionId}";
            var sub = armClient.GetSubscriptionResource(new ResourceIdentifier(subId));
            var env = config["DataHub_ENVNAME"];
            if (env is null)
            {
                logger.LogError("Environment name not found in configuration");
                throw new Exception("Environment name not found in configuration");
            }

            var regex = string.IsNullOrEmpty(workspaceAcronym)
                ? new Regex(
                    $"{FSDH_RG_PREFIX}[-_]({DBK_RG_KEY}|{PROJ_RG_KEY})[-_][a-z0-9]{{1,10}}[-_]{env}[-_]rg")
                : new Regex(
                    $"{FSDH_RG_PREFIX}[-_]({DBK_RG_KEY}|{PROJ_RG_KEY})[-_]{workspaceAcronym.ToLower()}[-_]{env}[-_]rg");

            var rgResources = new List<ResourceGroupResource>();
            var rgs = sub.GetResourceGroups();
            var pages = rgs.GetAllAsync().AsPages();
            await foreach (var page in pages)
            {
                var values = page.Values;
                foreach (var value in values)
                {
                    if (value is null || !value.HasData) continue;
                    var rgName = value.Data.Name;
                    if (string.IsNullOrEmpty(rgName)) continue;
                    if (!regex.IsMatch(rgName)) continue;
                    rgResources.Add(value);
                }
            }

            logger.LogInformation("{Count} resource groups in subscription {SubId}: {ResourceGroups}", rgResources.Count,
                subscriptionId, string.Join(", ", rgResources.Select(rg => rg.Data.Name)));
            return rgResources;
        }

        /// <summary>
        /// Small helper method to get the subscription id of a workspace
        /// </summary>
        /// <param name="workspaceAcronym">The workspace acronym to use</param>
        /// <returns>The subscription id as a string. i.e. "xxxx-xxxx-xxxx-xxxx"</returns>
        /// <exception cref="Exception">Will throw an exception if no subscription is found</exception>
        internal async Task<string> GetWorkspaceSubscriptionId(string workspaceAcronym)
        {
            using var ctx = await dbContextFactory.CreateDbContextAsync();
            var project = ctx.Projects.Include(p => p.DatahubAzureSubscription)
                .First(p => p.Project_Acronym_CD == workspaceAcronym);
            var sub = project.DatahubAzureSubscription;
            if (sub is null)
            {
                logger.LogError("Subscription not found for {WorkspaceAcronym}", workspaceAcronym);
                throw new Exception("Subscription not found");
            }

            return sub.SubscriptionId;
        }

        #endregion
    }
}