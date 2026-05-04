extern alias AzIdentity;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Datahub.Core.Model.Context;
using Datahub.Portal.Model;
using Datahub.Shared.Entities;
using Datahub.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Newtonsoft.Json;
using Azure.Identity;

namespace Datahub.Portal.Pages.Workspace.Database
{
    public partial class WorkspaceDatabasePage
    {
        /// <summary>
        /// Builds a PostgreSqlFlexibleServerResource object for the specified workspace acronym.
        /// </summary>
        /// <returns>A PostgreSqlFlexibleServerResource object.</returns>
        private async Task<PostgreSqlFlexibleServerResource> BuildPostgresSqlFlexibleServerResource()
        {
            var credential = new ClientSecretCredential(
                _portalConfiguration.AzureAd.TenantId,
                _portalConfiguration.AzureAd.InfraClientId,
                _portalConfiguration.AzureAd.InfraClientSecret);
            var client = new ArmClient(credential);

            var resourceGroupName =
                $"{_portalConfiguration.ResourcePrefix}_proj_{WorkspaceAcronym.ToLowerInvariant()}_{_portalConfiguration.Hosting.EnvironmentName}_rg";

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var subscriptionId = await RetrieveWorkspaceSubscriptionId(WorkspaceAcronym, context);
            var dbResource = await context.Project_Resources2.AsNoTracking().Include(p => p.Project).FirstAsync(r => r.ResourceType == TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres) && r.Project.Project_Acronym_CD == WorkspaceAcronym);
            var pgsqlId = dbResource.GetPostgresId();

            var postgresResource = client.GetPostgreSqlFlexibleServerResource(new ResourceIdentifier(pgsqlId));

            return postgresResource;
        }

        /// <summary>
        /// Retrieves the subscription ID for the specified workspace acronym.
        /// </summary>
        /// <param name="workspaceAcronym">The acronym of the workspace.</param>
        /// <param name="context">The database context to use for retrieving the workspace information.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the subscription ID.</returns>
        internal static async Task<string> RetrieveWorkspaceSubscriptionId(string workspaceAcronym,
            DatahubProjectDBContext context)
        {
            var workspace = await context.Projects
                .AsNoTracking()
                .Where(w => w.Project_Acronym_CD == workspaceAcronym)
                .Include(w => w.DatahubAzureSubscription)
                .FirstAsync();

            return workspace.DatahubAzureSubscription.SubscriptionId;
        }

    }
}
