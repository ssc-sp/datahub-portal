#nullable enable

using System.Text.Json;
using Datahub.Core.Model.Projects;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;

namespace Datahub.Core.Utils;

/// <summary>
/// Provides methods to extract information from a Terraform project.
/// </summary>
public static class TerraformVariableExtraction
{
    private static readonly JsonSerializerOptions CaseInsensitiveSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Extracts the Databricks workspace Id from the provided project.
    /// </summary>
    /// <param name="project">The Datahub project.</param>
    /// <returns>The Databricks workspace Id.</returns>
    public static string? ExtractDatabricksWorkspaceId(Datahub_Project? project)
    {
        var databricksTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        return ExtractStringVariable(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == databricksTemplateName)?.JsonContent,
            "workspace_id");
    }

    /// <summary>
    /// Extracts the databricks url from a Datahub Project. Be sure to include the project resources in the project object.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="isFrench">A flag indicating whether the Databricks preferred language is French</param>
    /// <returns>Databricks url of the project</returns>
    public static string? ExtractDatabricksUrl(Datahub_Project? project, bool? isFrench)
    {
        var databricksTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        var databricksUrlVariable = ExtractStringVariable(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == databricksTemplateName)?.JsonContent,
            "workspace_url");

        return AddLanguageURLParameter(FormatDatabricksUrl(databricksUrlVariable), isFrench);
    }

    private static string? AddLanguageURLParameter(string? databricksURL, bool? isFrench)
    {
        if (isFrench is null || databricksURL is null)
        {
            return databricksURL;
        }

        var langParam = $"login.html?l={((isFrench == true) ? "fr" : "en")}";
        if (databricksURL.EndsWith("/"))
        {
            databricksURL = databricksURL + langParam;
        }
        else
        {
            databricksURL = databricksURL + "/" + langParam;
        }

        return databricksURL;
    }

    /// <summary>
    /// Extracts the Databricks URL from the specified project resource.
    /// </summary>
    /// <param name="projectResource">The project resource containing the JSON content.</param>
    /// <param name="isFrench">A flag indicating whether the Databricks preferred language is French</param>
    /// <returns>The extracted Databricks URL or null if not found.</returns>
    public static string? ExtractDatabricksUrl(Project_Resources2? projectResource, bool isFrench)
    {
        var databricksUrlVariable = ExtractStringVariable(
            projectResource?.JsonContent,
            "workspace_url");

        return AddLanguageURLParameter(FormatDatabricksUrl(databricksUrlVariable), isFrench);
    }

    /// <summary>
    /// Extracts the app service configuration from a Datahub Project.
    /// </summary>
    /// <param name="project">The Datahub Project to find the app service config from</param>
    /// <returns>The AppServiceConfiguration object containing the configuration info of the app service</returns>
    public static AppServiceConfiguration? ExtractAppServiceConfiguration(Datahub_Project? project)
    {
        var appServiceTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureAppService);
        var appServiceResource = project?.Resources?.FirstOrDefault(r =>
            r.ResourceType == appServiceTemplateName);

        if (appServiceResource == null)
        {
            // TODO: Might be worth logging but this class seems to have all static functions and no logger.
            return null;
        }

        return ExtractAppServiceConfiguration(appServiceResource);
    }

    /// <summary>
    /// Extracts the app service configuration from a project resource.
    /// </summary>
    /// <param name="projectResource">The project resource</param>
    /// <returns>The AppServiceConfiguration object containing the configuration info of the app service</returns>
    public static AppServiceConfiguration ExtractAppServiceConfiguration(Project_Resources2? projectResource)
    {
        var appServiceFramework = ExtractStringVariable(
            projectResource?.InputJsonContent,
            "app_service_framework");
        var appServiceGitRepo = ExtractStringVariable(
            projectResource?.InputJsonContent,
            "app_service_git_repo");
        var appServiceGitRepoVisibility = bool.TryParse(
            ExtractStringVariable(
                projectResource?.InputJsonContent,
                "app_service_git_repo_visibility"),
            out var visibility) && visibility;
        var appServiceGitTokenSecretName = ExtractStringVariable(
            projectResource?.InputJsonContent,
            "app_service_git_token_secret_name");
        var appServiceComposePath = ExtractStringVariable(
            projectResource?.InputJsonContent,
            "app_service_compose_path");
        var appServiceId = ExtractStringVariable(
            projectResource?.JsonContent,
            "app_service_id");
        var appServiceHostName = ExtractStringVariable(
            projectResource?.JsonContent,
            "app_service_host_name");
        return new AppServiceConfiguration(appServiceFramework, appServiceGitRepo, appServiceComposePath, appServiceId,
            appServiceHostName, appServiceGitRepoVisibility, appServiceGitTokenSecretName);
    }

    /// <summary>
    /// Extracts the postgres configuration from a Datahub Project.
    /// </summary>
    /// <param name="project">The Datahub_Project record to extract from</param>
    /// <returns>A PostgresConfiguration object if the resource exists, null otherwise</returns>
    public static PostgresConfiguration? ExtractPostgresConfiguration(Datahub_Project? project)
    {
        var postgresTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        var postgresResource = project?.Resources?.FirstOrDefault(r =>
            r.ResourceType == postgresTemplateName);
        return postgresResource == null ? null : ExtractPostgresConfiguration(postgresResource);
    }

    /// <summary>
    /// Extracts the postgres configuration from a postgres project resource.
    /// </summary>
    /// <param name="projectResource">The terraform:azure-postgres resource to extract from</param>
    /// <returns>A PostgresConfiguration object</returns>
    public static PostgresConfiguration ExtractPostgresConfiguration(Project_Resources2? projectResource)
    {
        var postgresSku = ExtractStringVariable(
            projectResource?.InputJsonContent, "postgres_sku");

        return new PostgresConfiguration
        {
            PSQL_SKU = postgresSku
        };
    }

    /// <summary>
    /// Extracts the Databricks configuration from the specified project.
    /// </summary>
    /// <remarks>This method searches the project's resources for a Databricks resource and extracts
    /// configuration values from its input JSON content. If no Databricks resource is found,  the method returns <see
    /// langword="null"/>.</remarks>
    /// <param name="project">The project from which to extract the Databricks configuration.  This parameter can be <see langword="null"/>.</param>
    /// <returns>A <see cref="DatabricksConfiguration"/> object containing the extracted configuration,  or <see
    /// langword="null"/> if the project does not contain a Databricks resource.</returns>
    public static DatabricksConfiguration? ExtractDatabricksConfiguration(Datahub_Project? project)
    {
        var databricksTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        var databricksResource = project?.Resources?.FirstOrDefault(r =>
            r.ResourceType == databricksTemplateName);

        if (databricksResource != null)
        {
            var databricksConfiguration = JsonSerializer.Deserialize<DatabricksConfiguration>(
                databricksResource.InputJsonContent, CaseInsensitiveSerializerOptions);

            return databricksConfiguration;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts the environment variable keys from a project resource. This is made generic,
    /// so that it can be used on any resource
    /// </summary>
    /// <param name="projectResources">The project resource to get the environment variables from</param>
    /// <returns>A list of string, corresponding to the keys of the environment variables stored in the workspace keyvault</returns>
    public static IList<string> ExtractEnvironmentVariableKeys(Project_Resources2 projectResources)
    {
        var envVarsString = ExtractStringVariable(projectResources?.InputJsonContent, "environment_variables_keys") ??
                            "[]";
        var envVars = JsonSerializer.Deserialize<List<string>>(envVarsString);
        return envVars ?? new List<string>();
    }

    /// <summary>
    /// Formats a Databricks URL by adding "https://" if it's not already present.
    /// </summary>
    /// <param name="databricksUrlVariable">The Databricks URL to be formatted.</param>
    /// <returns>The formatted Databricks URL.</returns>
    private static string? FormatDatabricksUrl(string? databricksUrlVariable)
    {
        if (!databricksUrlVariable?.StartsWith("https://") ?? false)
        {
            databricksUrlVariable = $"https://{databricksUrlVariable}";
        }

        return databricksUrlVariable;
    }

    /// <summary>
    /// Extracts the Azure Postgres host from the given Datahub project.
    /// </summary>
    /// <param name="project">The Datahub project.</param>
    /// <returns>The Azure Postgres host, or null if not found.</returns>
    public static string? ExtractAzurePostgresHost(Datahub_Project? project)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractStringVariable(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent,
            "postgres_dns");
    }

    /// <summary>
    /// Extracts the name of the Azure PostgreSQL database from the specified workspace.
    /// </summary>
    /// <param name="workspace">The Datahub_Project object representing the workspace.</param>
    /// <returns>The name of the Azure PostgreSQL database.</returns>
    public static string? ExtractAzurePostgresDatabaseName(Datahub_Project? workspace)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractStringVariable(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent,
            "postgres_db_name");
    }

    /// <summary>
    /// Extracts the username secret name from the given Azure Postgres workspace.
    /// </summary>
    /// <param name="workspace">The Azure Postgres workspace from which to extract the username secret name.</param>
    /// <returns>The extracted username secret name.</returns>
    public static string? ExtractAzurePostgresUsernameSecretName(Datahub_Project? workspace)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractStringVariable(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent,
            "postgres_secret_name_admin");
    }

    /// <summary>
    /// Extracts the name of the Azure PostgreSQL password secret from a Datahub workspace.
    /// </summary>
    /// <param name="workspace">The Datahub workspace.</param>
    /// <returns>The name of the Azure PostgreSQL password secret, or null if not found.</returns>
    public static string? ExtractAzurePostgresPasswordSecretName(Datahub_Project? workspace)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractStringVariable(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent,
            "postgres_secret_name_password");
    }

    /// <summary>
    /// Extracts the resource group name associated with a workspace from the provided project.
    /// </summary>
    /// <param name="workspace">The Datahub project containing the workspace information.</param>
    /// <returns>The resource group name of the workspace if available; otherwise, null.</returns>
    public static string? ExtractWorkspaceResourceGroupName(Datahub_Project? workspace)
    {
        var resourceGroupTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.NewProjectTemplate);
        return ExtractStringVariable(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == resourceGroupTemplateName)?.JsonContent,
            "resource_group_name");
    }

    /// <summary>
    /// Extracts the Azure Storage Account name from the provided project.
    /// </summary>
    /// <param name="workspace">The Datahub project.</param>
    /// <returns>The Azure Storage Account name, or null if not found.</returns>
    public static string? ExtractAzureStorageAccountName(Datahub_Project? workspace)
    {
        var storageAccountTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureStorageBlob);
        return ExtractStringVariable(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == storageAccountTemplateName)?.JsonContent,
            "storage_account");
    }

    private static Project_Resources2? GetProjectResource(Datahub_Project? workspace, string terraformTemplateName)
    {
        var fullTemplateName = TerraformTemplate.GetTerraformServiceType(terraformTemplateName);
        return workspace?.Resources?.FirstOrDefault(r => r.ResourceType == fullTemplateName);
    }

    /// <summary>
    /// Checks if a specified resource has been requested for the given workspace.
    /// </summary>
    /// <param name="workspace">The Datahub workspace.</param>
    /// <param name="terraformTemplateName">The template name for the resource. See <see cref="TerraformTemplate"/> for template names.</param>
    /// <returns>True if the resource has been requested and not deleted</returns>
    public static bool IsResourceRequested(Datahub_Project? workspace, string terraformTemplateName)
    {
        var resource = GetProjectResource(workspace, terraformTemplateName);
        if (resource is null)
        {
            return false;
        }

        return TerraformStatus.CreatedOrInProcessOf(resource.Status);
    }

    /// <summary>
    /// Extracts a string variable from the given JSON content based on the variable name.
    /// </summary>
    /// <param name="projectResourceJsonContent">The JSON content from which to extract the variable.</param>
    /// <param name="variableName">The name of the variable to extract.</param>
    /// <returns>The extracted string variable.</returns>
    private static string? ExtractStringVariable(string? projectResourceJsonContent, string variableName)
    {
        if (string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;

        var deserializeOptions = CaseInsensitiveSerializerOptions;

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);

        if (!jsonContent?.ContainsKey(variableName) ?? true)
            return null;

        var variable = jsonContent?[variableName];

        return variable;
    }
}