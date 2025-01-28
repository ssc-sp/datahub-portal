using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using Azure.ResourceManager.Storage;
using Datahub.Application.Configuration;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Application.Services.Storage;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Workspace;
using Datahub.Core.Utils;
using Datahub.Infrastructure.Services.Helpers;
using Datahub.Shared.Clients;
using Datahub.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.SqlClient;

namespace Datahub.Infrastructure.Services.Storage
{
    public class DatabaseManagementService(
        ILogger<WorkspaceStorageManagementService> logger,
        IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
        DatahubPortalConfiguration portalConfiguration)
        : IDatabaseManagementService
    {
        private string AzureTenantId => portalConfiguration.AzureAd.TenantId;
        private string DevopsClientId => portalConfiguration.AzureAd.InfraClientId;
        private string DevopsClientSecret => portalConfiguration.AzureAd.InfraClientSecret;
        private string SubscriptionId => portalConfiguration.AzureAd.SubscriptionId;

        private AzureDevOpsConfiguration BuildDevopsConfig() => new()
        {
            ClientId = DevopsClientId,
            ClientSecret = DevopsClientSecret,
            TenantId = AzureTenantId
        };
        #region Implementations

        /// <inheritdoc />
        public async Task<DatabaseInfo> GetDatabaseInfo(string workspaceAcronym)
        {
            var reply = new DatabaseInfo();

            logger.LogInformation("Getting database info for workspace {WorkspaceAcronym}", workspaceAcronym);
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var workspace = await context.Projects
                .Include(x => x.Resources)
                .FirstOrDefaultAsync(x => x.Project_Acronym_CD == workspaceAcronym);

            if (workspace == null)
                throw new Exception($"Workspace with acronym {workspaceAcronym} not found");

            var databaseHost = TerraformVariableExtraction.ExtractAzurePostgresHost(workspace);
            var databaseName = TerraformVariableExtraction.ExtractAzurePostgresDatabaseName(workspace);
            var postgresUsername = TerraformVariableExtraction.ExtractAzurePostgresUsernameSecretName(workspace);
            var postgresPassword = TerraformVariableExtraction.ExtractAzurePostgresPasswordSecretName(workspace);

            var connectionString = $"Server={databaseHost}.database.windows.net;Database={databaseName};";
            var pgConnectionString = $"Host={databaseHost}.postgres.database.azure.com;Database={databaseName};Username={postgresUsername};Password={postgresPassword}";

            var azureDevOpsClient = new AzureDevOpsClient(BuildDevopsConfig());
            var accessToken = await azureDevOpsClient.AccessTokenAsync(false,true);

            var credential = new DefaultAzureCredential();
            var connection = new SqlConnection(connectionString)
            {
                AccessToken = accessToken.Token
            };

            reply.Connection = connectionString;
            connection.Open();

            // Retrieve general database configuration
            using (var command = new SqlCommand("SELECT service_objective, edition, physical_database_name FROM sys.database_service_objectives", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reply.ServiceObjective = reader["service_objective"].ToString();
                        reply.Edition = reader["edition"].ToString(); 
                    }
                }
            }

            connection.Close();

            // retrieve Postgres specific data
            var pgConnection = new NpgsqlConnection(pgConnectionString);

            pgConnection.Open();

            // Retrieve PostgreSQL version
            using (var command = new NpgsqlCommand("SELECT version()", pgConnection))
            {
                var version = command.ExecuteScalar().ToString();
                reply.PSQLVersion = version;
            }

            // Retrieve physical setup details
            using (var command = new NpgsqlCommand("SELECT pg_size_pretty(pg_database_size(current_database())) AS size, pg_tablespace_location(pg_tablespace.oid) AS location FROM pg_tablespace WHERE spcname = 'pg_default'", pgConnection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reply.Size = reader["size"].ToString();
                        reply.Location = reader["location"].ToString(); 
                    }
                }
            }

            pgConnection.Close(); 
            return reply;
        }

        #endregion
    }
}