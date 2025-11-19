using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Datahub.Shared.Entities.WorkspaceToolConfiguration
{
    public class PostgresConfiguration : IWorkspaceToolConfiguration, IWorkspaceToolWithSuffix
    {
        public string PSQL_SKU { get; set; } = PostgresTier.DefaultTier.PSQL_SKU;
        public string? ResourceNameSuffix { get; set; }

        public static string GetPropertyLabel(string propertyName) => propertyName switch
        {
            nameof(PSQL_SKU) => "Database tier",
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
                ["postgres_sku"] = PSQL_SKU
            };
            return postgresJson.ToString();
        }

        public void WriteToWorkspaceDefinition(WorkspaceDefinition workspaceDefinition)
        {
            workspaceDefinition.AppData.PostgresConfiguration = this;
        }
    }
}
