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

    public record struct TemplateStatus
    {
        public const string CreateRequested = "CreateRequested";
        public const string Created = "Created";
        public const string DeleteRequested = "DeleteRequested";
        public const string Deleted = "Deleted";
        public const string Unknown = "Unknown";
    }

    public string Name { get; } = name;

    public string Status { get; } = status ?? TemplateStatus.Unknown;

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
            AzureAppService => AzureDatabricks,
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
                new TerraformTemplate(AzureStorageBlob, TemplateStatus.CreateRequested),
            ],
            AzureAppService =>
            [
                new TerraformTemplate(AzureStorageBlob, TemplateStatus.CreateRequested),
            ],
            AzurePostgres => [],
            VariableUpdate => [],
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }
}