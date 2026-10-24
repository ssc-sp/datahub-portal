using System.Text.Json.Serialization;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;

namespace Datahub.Shared.Entities;

public class TerraformTemplate
{
    private static readonly string[] SupportedTemplateNames =
    [
        NewProjectTemplate,
        NewProjectTemplateEnhanced,
        VariableUpdate,
        AzureDatabricks,
        AzureDatabricksEnhanced,
        AzureVirtualMachine,
        AzureAppService,
        AzureAppServiceEnhanced,
        AzurePostgres,
        AzurePostgresEnhanced,
        ContactUs,
        AzureArcGis,
        AzureAPI,
        AzureAPIEnhanced,
        AzurePostgresTemplateFile,
        "azure-storage-blob"
    ];

    private static readonly HashSet<string> ReleasedTemplateNames =
    [
        NewProjectTemplate,
        NewProjectTemplateEnhanced,
        VariableUpdate,
        AzureDatabricks,
        AzureDatabricksEnhanced,
        AzureAppService,
        AzureAppServiceEnhanced,
        AzurePostgres,
        AzurePostgresEnhanced
    ];

    public static string GetTerraformServiceType(string templateName) => $"terraform:{templateName}";

    public static bool IsReleased(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var normalizedName = NormalizeTemplateName(name);
        return ReleasedTemplateNames.Contains(normalizedName, StringComparer.OrdinalIgnoreCase);
    }

    public const string NewProjectTemplate = "new-project-template";
    public const string NewProjectTemplateEnhanced = "new-project-template-enhanced";
    public const string VariableUpdate = "variable-update";
    public const string AzureDatabricks = "azure-databricks";
    public const string AzureDatabricksEnhanced = "azure-databricks-enhanced";
    public const string AzureVirtualMachine = "azure-virtual-machine";
    public const string AzureAppService = "azure-app-service";
    public const string AzureAppServiceEnhanced = "azure-app-service-enhanced";
    public const string AzurePostgres = "azure-postgres";
    public const string AzurePostgresEnhanced = "azure-postgres-enhanced";
    public const string ContactUs = "contact-us";
    public const string AzureArcGis = "azure-arcgis";
    public const string AzureAPI = "azure-api";
    public const string AzureAPIEnhanced = "azure-api-enhanced";
    public const string AzurePostgresTemplateFile = "azure-postgresql";

    public const string NewProjectTemplateFile = "main";

    // Parameterless constructor for deserialization
    public TerraformTemplate()
    {
        Name = string.Empty;
        Status = TerraformStatus.Unknown;
        RequestedAt = null;
    }

    // Primary constructor
    [JsonConstructor]
    public TerraformTemplate(string name, string status)
    {
        Name = name;
        Status = status ?? TerraformStatus.Unknown;
        RequestedAt = null;
    }

    // Constructor with requestedAt for internal use
    public TerraformTemplate(string name, string status, DateTime requestedAtUtc)
    {
        Name = name;
        Status = status ?? TerraformStatus.Unknown;
        RequestedAt = requestedAtUtc;
    }

    public string Name { get; set; }

    public string Status { get; set; }

    [JsonIgnore]
    public DateTime? RequestedAt { get; set; }

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
            AzureDatabricks => "Azure Databricks",
            AzureAppService => "Azure App Service",
            AzurePostgres => "Azure Postgres",
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }

    public static string NormalizeTemplateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Template name cannot be null or empty.", nameof(name));
        }

        var templateName = name.Trim();

        if (templateName.StartsWith("terraform:", StringComparison.OrdinalIgnoreCase))
        {
            templateName = templateName["terraform:".Length..];
        }

        var normalizedName = templateName.Trim();
        var match = SupportedTemplateNames.FirstOrDefault(t =>
            string.Equals(t, normalizedName, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
        {
            return match;
        }

        throw new ArgumentException($"Unknown template name: {name}");
    }

#nullable enable
    public static IEnumerable<string>? GetDependencyNames(string toolName)
    {
        var normalizedName = NormalizeTemplateName(toolName);

        var dependencyMap = new Dictionary<string, string[]>
        {
            [NewProjectTemplate] = [],
            [NewProjectTemplateEnhanced] = [NewProjectTemplate],
            [VariableUpdate] = [],
            [AzureDatabricks] = [NewProjectTemplate],
            [AzureDatabricksEnhanced] = [NewProjectTemplateEnhanced],
            [AzureVirtualMachine] = [NewProjectTemplate],
            [AzureAppService] = [NewProjectTemplate],
            [AzureAppServiceEnhanced] = [NewProjectTemplateEnhanced],
            [AzurePostgres] = [NewProjectTemplate],
            [AzurePostgresEnhanced] = [NewProjectTemplateEnhanced],
            [ContactUs] = [NewProjectTemplate],
            [AzureArcGis] = [NewProjectTemplate],
            [AzureAPI] = [NewProjectTemplate],
            [AzureAPIEnhanced] = [NewProjectTemplateEnhanced],
            [AzurePostgresTemplateFile] = [NewProjectTemplate]
        };

        return dependencyMap.TryGetValue(normalizedName, out var dependencies)
            ? dependencies
            : null;
    }
#nullable disable

    public static List<TerraformTemplate> GetDependenciesToCreate(string name)
    {
        return GetDependencyNames(name)
            .Select(t => new TerraformTemplate(t, TerraformStatus.CreateRequested, DateTime.UtcNow))
            .ToList() ?? throw new ArgumentException($"Unknown template name: {name}");
    }

    public static string MapHealthResourceTypeToTemplateConstant(InfrastructureHealthResourceType resourceType)
    {
        return resourceType switch
        {
            InfrastructureHealthResourceType.AzureDatabricks => GetTerraformServiceType(AzureDatabricks),
            InfrastructureHealthResourceType.AzureWebApp => GetTerraformServiceType(AzureAppService),
            InfrastructureHealthResourceType.AzureSqlDatabase => GetTerraformServiceType(AzurePostgres),
            _ => string.Empty,
        };
    }
}
