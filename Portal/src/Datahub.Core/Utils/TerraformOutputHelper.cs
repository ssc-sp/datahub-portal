using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;

namespace Datahub.Core.Utils;

public static class TerraformOutputHelper
{
    public static string GetExpectedTerraformOutput(Datahub_Project project, string workspaceId = null, string workspaceUrl = null, string envAcronym = "dev")
    {
        var expectedTerraformOutput = GetExpectedTerraformOutputResourceGroupString();
        // var workspaceId = "";
        // var workspaceUrl = "";

        if (project.Resources.Any(r => r.ResourceType.Equals(TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob))))
        {
            expectedTerraformOutput =
                string.Join(",", expectedTerraformOutput, GetExpectedTerraformOutputAzureStorageString());
        }

        if (project.Resources.Any(r => r.ResourceType.Equals(TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks))))
        {
            expectedTerraformOutput = string.Join(",", expectedTerraformOutput,
                GetExpectedTerraformOutputAzureDatabricksString());
            workspaceId ??= TerraformVariableExtraction.ExtractDatabricksWorkspaceId(project);
            workspaceUrl ??= TerraformVariableExtraction.ExtractDatabricksUrl(project);
        }

        if (project.Resources.Any(r => r.ResourceType.Equals(TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService))))
        {
            expectedTerraformOutput =
                string.Join(",", expectedTerraformOutput, GetExpectedTerraformOutputAzureWebAppString());
        }

        if (project.Resources.Any(r => r.ResourceType.Equals(TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres))))
        {
            expectedTerraformOutput =
                string.Join(",", expectedTerraformOutput, GetExpectedTerraformOutputAzurePostgresString());
        }

        var content = expectedTerraformOutput
            .Replace("{{env_acronym}}", envAcronym)
            .Replace("{{project_cd}}", project.Project_Acronym_CD)
            .Replace("{{project_cd_lower}}", project.Project_Acronym_CD.ToLower())
            .Replace("{{workspace_version}}", project.Version)
            .Replace("{{workspace_id}}", workspaceId)
            .Replace("{{workspace_url}}", workspaceUrl);

        return $"{{\n{content}\n}}";
    }

    private static string GetExpectedTerraformOutputAzurePostgresString()
    {
        return @"  ""azure_postgresql_db_name"": {
        ""sensitive"": false,
        ""type"": ""string"",
        ""value"": ""fsdh""
    },
    ""azure_postgresql_dns"": {
        ""sensitive"": false,
        ""type"": ""string"",
        ""value"": ""fsdh-{{project_cd_lower}}-psql-{{env_acronym}}.postgres.database.azure.com""
    },
    ""azure_postgresql_id"": {
        ""sensitive"": false,
        ""type"": ""string"",
        ""value"": ""/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_{{project_cd_lower}}_{{env_acronym}}_rg/providers/Microsoft.DBforPostgreSQL/flexibleServers/fsdh-{{project_cd_lower}}-psql-{{env_acronym}}""
    },
    ""azure_postgresql_secret_name_admin"": {
        ""sensitive"": false,
        ""type"": ""string"",
        ""value"": ""datahub-psql-admin""
    },
    ""azure_postgresql_secret_name_password"": {
        ""sensitive"": false,
        ""type"": ""string"",
        ""value"": ""datahub-psql-password""
    },
    ""azure_postgresql_server_name"": {
        ""sensitive"": false,
        ""type"": ""string"",
        ""value"": ""fsdh-{{project_cd_lower}}-psql-{{env_acronym}}""
    },
    ""azure_psql_module_status"": {
        ""sensitive"": false,
        ""type"": ""string"",
        ""value"": ""completed""
    }";
    }

    private static string GetExpectedTerraformOutputAzureWebAppString()
    {
        return @"  ""azure_app_service_hostname"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""fsdh-proj-{{project_cd_lower}}-webapp-{{env_acronym}}.azurewebsites.net""
  },
  ""azure_app_service_id"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_{{project_cd_lower}}_{{env_acronym}}_rg/providers/Microsoft.Web/sites/fsdh-proj-{{project_cd_lower}}-webapp-{{env_acronym}}""
  },
  ""azure_app_service_module_status"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""completed""
  }";
    }

    private static string GetExpectedTerraformOutputAzureDatabricksString()
    {
        return @"  ""azure_databricks_module_status"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""completed""
  },
  ""azure_databricks_workspace_id"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""{{workspace_id}}""
  },
  ""azure_databricks_workspace_name"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""fsdh-dbk-{{project_cd_lower}}-{{env_acronym}}""
  },
  ""azure_databricks_workspace_url"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""{{workspace_url}}""
  }";
    }

    private static string GetExpectedTerraformOutputAzureStorageString()
    {
        return @"  ""azure_storage_account_name"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""fsdhproj{{project_cd_lower}}{{env_acronym}}""
  },
  ""azure_storage_blob_status"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""completed""
  },
  ""azure_storage_container_name"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""datahub""
  }";
    }

    private static string GetExpectedTerraformOutputResourceGroupString()
    {
        return @"  ""new_project_template"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""completed""
  },
  ""azure_resource_group_name"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""fsdh_proj_{{project_cd_lower}}_{{env_acronym}}_rg""
  },
  ""project_cd"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""{{project_cd}}""
  },
  ""workspace_version"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""{{workspace_version}}""
  }";
    }
}