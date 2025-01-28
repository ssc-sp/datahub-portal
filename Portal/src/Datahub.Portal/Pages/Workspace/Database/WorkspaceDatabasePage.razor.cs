using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Datahub.Core.Model.Context;
using Datahub.Portal.Model;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using Newtonsoft.Json;

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
            var resourceProviderNamespace = "Microsoft.DBforPostgreSQL";
            var resourceType = "flexibleServers";
            var resourceName =
                $"{_portalConfiguration.ResourcePrefix}-{WorkspaceAcronym.ToLowerInvariant()}-psql-{_portalConfiguration.Hosting.EnvironmentName}";

            var resourceIdentifier =
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/{resourceProviderNamespace}/{resourceType}/{resourceName}";

            var postgresResource = client.GetPostgreSqlFlexibleServerResource(new ResourceIdentifier(resourceIdentifier));

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

        /// <summary>
        /// Loads list of compute resources available
        /// see https://learn.microsoft.com/en-us/azure/postgresql/flexible-server/concepts-compute
        /// </summary>
        /// <returns>List of Azure specifications</returns>
        internal async Task<List<AzureDBServer>> LoadSKUs()
        {
            try
            {
                var assembly = typeof(Program).Assembly;
                var resourceName = "Datahub.Portal.Data.AzureFlexServers.json";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                using var reader = new StreamReader(stream);
                var jsonData = await reader.ReadToEndAsync();
                var data = JsonConvert.DeserializeObject<List<AzureDBServer>>(jsonData);
                return data;
            }
            catch (Exception x)
            {
                _logger.LogError(x.Message);
                return null;
            }
        }
    }
}
