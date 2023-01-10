using Datahub.Shared;
using Datahub.Shared.Entities;

namespace Datahub.ProjectTools.Utils;

using System.Text.Json;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Projects;
using Datahub.ProjectTools.Services;

public static class TerraformVariableExtraction
{

    /// <summary>
    /// Extracts the databricks url from a Datahub Project. Be sure to include the project resources in the project object.
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static string? ExtractDatabricksUrl(Datahub_Project? project)
    {
        var databricksTemplateName = RequestManagementService.GetTerraformServiceType(TerraformTemplate.AzureDatabricks);
        return ExtractDatabricksUrl(
            project?.Resources?.FirstOrDefault(r => r.ResourceType == databricksTemplateName)?.JsonContent);
    }
    
    /// <summary>
    /// Parses the project resource content to return the databricks url.
    /// </summary>
    /// <param name="projectResourceJsonContent"></param>
    /// <returns></returns>a
    private static string? ExtractDatabricksUrl(string? projectResourceJsonContent)
    {
        if(string.IsNullOrWhiteSpace(projectResourceJsonContent))
            return null;
        
        var deserializeOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var jsonContent =
            JsonSerializer.Deserialize<Dictionary<string, string>>(projectResourceJsonContent, deserializeOptions);
        var databricksUrlVariable = jsonContent?["workspace_url"];
        
        return databricksUrlVariable;
    }
}