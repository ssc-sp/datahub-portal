using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration
{
    public class PostgresConfiguration : IWorkspaceToolConfiguration, IWorkspaceToolWithSuffix
    {
        public const string PGSQL_JSON_SKU = "postgres_sku";
        public const string PGSQL_JSON_SUFFIX = "postgres_suffix";

        public string PSQL_SKU { get; set; } = PostgresTier.DefaultTier.PSQL_SKU;
        public string? ResourceNameSuffix { get; set; }

        public static string GetPropertyLabel(string propertyName) => propertyName switch
        {
            nameof(PSQL_SKU) => "Database tier",
            nameof(ResourceNameSuffix) => "Resource name suffix",
            _ => propertyName
        };

        public static IWorkspaceToolConfiguration ReadFromWorkspaceDefinition(WorkspaceDefinition workspaceDefinition)
        {
            return workspaceDefinition.AppData.PostgresConfiguration ?? new PostgresConfiguration();
        }

        public IWorkspaceToolConfiguration Clone() => new PostgresConfiguration()
        {
            PSQL_SKU = PSQL_SKU,
            ResourceNameSuffix = ResourceNameSuffix
        };

        public string GenerateResourceInputJson()
        {
            var postgresJson = new JsonObject
            {
                [PGSQL_JSON_SKU] = PSQL_SKU,
                [PGSQL_JSON_SUFFIX] = ResourceNameSuffix
            };
            return postgresJson.ToString();
        }

        public void WriteToWorkspaceDefinition(WorkspaceDefinition workspaceDefinition)
        {
            workspaceDefinition.AppData.PostgresConfiguration = this;
        }
    }
}
