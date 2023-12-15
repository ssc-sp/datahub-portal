using System.Linq;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Utils;

public static class TerraformOutputHelper
{
    public static string GetExpectedTerraformOutput(Datahub_Project project)
    {
        var expectedTerraformOutput = GetExpectedTerraformOutputResourceGroupString();
        var workspaceId = "";
        var workspaceUrl = "";

        if (project.Resources.Any(r => r.ResourceType.Equals("terraform:azure-storage-blob")))
        {
            expectedTerraformOutput =
                string.Join(",", expectedTerraformOutput, GetExpectedTerraformOutputAzureStorageString());
        }

        if (project.Resources.Any(r => r.ResourceType.Equals("terraform:azure-databricks")))
        {
            expectedTerraformOutput = string.Join(",", expectedTerraformOutput,
                GetExpectedTerraformOutputAzureDatabricksString());
            workspaceId = TerraformVariableExtraction.ExtractDatabricksWorkspaceId(project);
            workspaceUrl = TerraformVariableExtraction.ExtractDatabricksUrl(project);
        }

        if (!string.IsNullOrWhiteSpace(project.WebApp_URL))
        {
            expectedTerraformOutput =
                string.Join(",", expectedTerraformOutput, GetExpectedTerraformOutputAzureWebAppString());
        }

        var content = expectedTerraformOutput
            .Replace("{{project_cd}}", project.Project_Acronym_CD)
            .Replace("{{project_cd_lower}}", project.Project_Acronym_CD.ToLower())
            .Replace("{{workspace_version}}", project.Version)
            .Replace("{{workspace_id}}", workspaceId)
            .Replace("{{workspace_url}}", workspaceUrl);

        return $"```json\n{{\n{content}\n}}\n```";
    }

    private static string GetExpectedTerraformOutputAzureWebAppString()
    {
        return @"  ""azure_app_service_hostname"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""fsdh-proj-{{project_cd_lower}}-webapp-poc.azurewebsites.net""
  },
  ""azure_app_service_id"": {
    ""sensitive"": false,
    ""type"": ""string"",
    ""value"": ""/subscriptions/bc4bcb08-d617-49f4-b6af-69d6f10c240b/resourceGroups/fsdh_proj_{{project_cd_lower}}_poc_rg/providers/Microsoft.Web/sites/fsdh-proj-{{project_cd_lower}}-webapp-poc""
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
    ""value"": ""fsdh-dbk-{{project_cd_lower}}-poc""
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
    ""value"": ""fsdhproj{{project_cd_lower}}poc""
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
    ""value"": ""fsdh_proj_{{project_cd_lower}}_poc_rg""
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