#nullable enable

using System.Collections.Generic;
using System.Linq;
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
    public static string? ExtractDatabricksWorkspaceId(Datahub_Project? project)
    {
        var databricksTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        return ExtractDatabricksWorkspaceId(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == databricksTemplateName)?.JsonContent);
    }

    /// <summary>
    /// Extracts the Databricks workspace ID from the given project resource JSON content.
    /// </summary>
    /// <param name="projectResourceJsonContent">The project resource JSON content.</param>
    /// <returns>The Databricks workspace ID if found; otherwise, null.</returns>
    private static string? ExtractDatabricksWorkspaceId(string? projectResourceJsonContent)
    {
        if (string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);
        var databricksWorkspaceIdVariable = jsonContent?["workspace_id"];

        return databricksWorkspaceIdVariable;
    }

    /// <summary>
    /// Extracts the databricks url from a Datahub Project. Be sure to include the project resources in the project object.
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static string? ExtractDatabricksUrl(Datahub_Project? project)
    {
        var databricksTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        return ExtractDatabricksUrl(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == databricksTemplateName)?.JsonContent);
    }

    /// <summary>
    /// Parses the project resource content to return the databricks url.
    /// </summary>
    /// <param name="projectResourceJsonContent"></param>
    /// <returns></returns>a
    public static string? ExtractDatabricksUrl(string? projectResourceJsonContent)
    {
        if (string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);
        var databricksUrlVariable = jsonContent?["workspace_url"];

        if (!databricksUrlVariable?.StartsWith("https://") ?? false)
        {
            databricksUrlVariable = $"https://{databricksUrlVariable}";
        }

        return databricksUrlVariable;
    }

    public static string? ExtractAzurePostgresHost(Datahub_Project? project)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractAzurePostgresHost(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent);
    }

    private static string? ExtractAzurePostgresHost(string? projectResourceJsonContent)
    {
        if (string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);
        var azureDatabaseHostVariable = jsonContent?["postgres_dns"];

        return azureDatabaseHostVariable;
    }

    public static string? ExtractAzurePostgresDatabaseName(Datahub_Project? workspace)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractAzurePostgresDatabaseName(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent);
    }

    private static string? ExtractAzurePostgresDatabaseName(string? projectResourceJsonContent)
    {
        if (string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);
        var azureDatabaseNameVariable = jsonContent?["postgres_db_name"];

        return azureDatabaseNameVariable;
    }

    public static string? ExtractAzurePostgresUsernameSecretName(Datahub_Project? workspace)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractAzurePostgresUsernameSecretName(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent);
    }

    private static string? ExtractAzurePostgresUsernameSecretName(string? projectResourceJsonContent)
    {
        if (string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);
        var azureDatabaseUsernameSecretNameVariable = jsonContent?["postgres_secret_name_admin"];

        return azureDatabaseUsernameSecretNameVariable;
    }

    public static string? ExtractAzurePostgresPasswordSecretName(Datahub_Project? workspace)
    {
        var azureDatabaseTemplateName = TerraformTemplate.GetTerraformServiceType(TerraformTemplate.AzurePostgres);
        return ExtractAzurePostgresPasswordSecretName(
            workspace?.Resources?.FirstOrDefault(r => r.ResourceType == azureDatabaseTemplateName)?.JsonContent);
    }

    private static string? ExtractAzurePostgresPasswordSecretName(string? projectResourceJsonContent)
    {
        if (string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;

        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);
        var azureDatabasePasswordSecretNameVariable = jsonContent?["postgres_secret_name_password"];

        return azureDatabasePasswordSecretNameVariable;
    }
}