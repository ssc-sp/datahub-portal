namespace Datahub.Shared.Entities;

public class TerraformTemplate(string name, string status)
{
    public static string GetTerraformServiceType(string templateName) => $"terraform:{templateName}";

    public const string NewProjectTemplate = "new-project-template";
    public const string VariableUpdate = "variable-update";
    public const string AzureStorageBlob = "azure-storage-blob";
    public const string AzureDatabricks = "azure-databricks";
    public const string AzureVirtualMachine = "azure-virtual-machine";
    public const string AzureAppService = "azure-app-service";
    public const string AzurePostgres = "azure-postgres";
    public const string ContactUs = "contact-us";
    public const string AzureArcGis = "azure-arcgis";
    public const string AzureAPI = "azure-api";

    public string Name { get; } = name;

    public string Status { get; } = status ?? TerraformStatus.Unknown;

    /// <summary>
    /// Converts a template name to a readable name based on the specified culture.
    /// </summary>
    /// <param name="name">The name of the template to be converted.</param>
    /// <param name="isFrench">The culture to use for translation. Default is english.</param>
    /// <returns>A readable name for the template in the specified culture.</returns>
    /// <exception cref="ArgumentException">Thrown when the culture is unknown or the template name is unknown.</exception>
    public static string ConvertTemplateNameToReadableName(string name, bool isFrench = false)
    {
        return isFrench
            ? ConvertTemplateNameToReadableNameFr(name)
            : ConvertTemplateNameToReadableNameEn(name);
    }

    /// <summary>
    /// Converts a template name to a readable name in French.
    /// </summary>
    /// <param name="name">The name of the template to be converted.</param>
    /// <returns>A readable name for the template in French.</returns>
    /// <exception cref="ArgumentException">Thrown when the template name is unknown.</exception>
    private static string ConvertTemplateNameToReadableNameFr(string name)
    {
        var normalizedName = NormalizeTemplateName(name);
        return normalizedName switch
        {
            NewProjectTemplate => "Groupe de ressources de l'espace de travail",
            AzureStorageBlob => "Blob de stockage Azure",
            AzureDatabricks => "Azure Databricks",
            AzureAppService => "Service App Azure",
            AzurePostgres => "Azure Postgres",
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }

    /// <summary>
    /// Converts a template name to a readable name in English.
    /// </summary>
    /// <param name="name">The name of the template to be converted.</param>
    /// <returns>A readable name for the template in English.</returns>
    /// <exception cref="ArgumentException">Thrown when the template name is unknown.</exception>
    private static string ConvertTemplateNameToReadableNameEn(string name)
    {
        var normalizedName = NormalizeTemplateName(name);
        return normalizedName switch
        {
            NewProjectTemplate => "Workspace Resource Group",
            AzureStorageBlob => "Azure Storage Blob",
            AzureDatabricks => "Azure Databricks",
            AzureAppService => "Azure App Service",
            AzurePostgres => "Azure Postgres",
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }

    public static string NormalizeTemplateName(string name)
    {
        var templateName = name.ToLower();
        if (templateName.StartsWith("terraform:"))
        {
            templateName = templateName.Replace("terraform:", string.Empty);
        }

        return templateName switch
        {
            NewProjectTemplate => NewProjectTemplate,
            AzureStorageBlob => AzureStorageBlob,
            AzureDatabricks => AzureDatabricks,
            AzureAppService => AzureAppService,
            AzurePostgres => AzurePostgres,
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }

    public static List<TerraformTemplate> GetDependenciesToCreate(string name)
    {
        return name switch
        {
            NewProjectTemplate => [],
            AzureStorageBlob => [],
            AzureDatabricks =>
            [
                new TerraformTemplate(AzureStorageBlob, TerraformStatus.CreateRequested),
            ],
            AzureAppService =>
            [
                new TerraformTemplate(AzureStorageBlob, TerraformStatus.CreateRequested),
            ],
            AzurePostgres => [],
            VariableUpdate => [],
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }
}