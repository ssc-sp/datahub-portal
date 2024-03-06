#nullable enable

using System.Text.Json;
using Datahub.Core.Model.Projects;
using Datahub.Shared.Entities;

namespace Datahub.Core.Utils;

/// <summary>
/// Provides methods to extract information from a Terraform project.
/// </summary>
public static class TerraformVariableExtraction
{
    /// <summary>
    /// Extracts the Databricks workspace Id from the provided project.
    /// </summary>
    /// <param name="project">The Datahub project.</param>
    /// <returns>The Databricks workspace Id.</returns>
    public static string? ExtractDatabricksWorkspaceId(DatahubProject? project)
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
    /// <returns>Databricks URL of the workspace</returns>
    public static string? ExtractDatabricksUrl(DatahubProject? project)
    {
        var databricksTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        var databricksUrlVariable = ExtractStringVariable(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == databricksTemplateName)?.JsonContent,
            "workspace_url");

        return FormatDatabricksUrl(databricksUrlVariable);
    }

    /// <summary>
    /// Extracts the Databricks URL from the specified project resource.
    /// </summary>
    /// <param name="projectResource">The project resource containing the JSON content.</param>
    /// <returns>The extracted Databricks URL or null if not found.</returns>
    public static string? ExtractDatabricksUrl(ProjectResources2? projectResource)
    {
        var databricksUrlVariable = ExtractStringVariable(
            projectResource?.JsonContent,
            "workspace_url");

        return FormatDatabricksUrl(databricksUrlVariable);
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
    public static string? ExtractAzurePostgresHost(DatahubProject? project)
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
    public static string? ExtractAzurePostgresDatabaseName(DatahubProject? workspace)
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
    public static string? ExtractAzurePostgresUsernameSecretName(DatahubProject? workspace)
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
    public static string? ExtractAzurePostgresPasswordSecretName(DatahubProject? workspace)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractStringVariable(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent,
            "postgres_secret_name_password");
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

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);

        if (!jsonContent?.ContainsKey(variableName) ?? true)
            return null;

        var variable = jsonContent?[variableName];

        return variable;
    }
}