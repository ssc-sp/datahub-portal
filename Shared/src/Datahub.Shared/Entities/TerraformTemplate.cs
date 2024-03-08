using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Shared.Entities;

public class TerraformTemplate
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

    public string Name { get; set; }
    public static TerraformTemplate Default => LatestFromName(NewProjectTemplate);

    public static TerraformTemplate LatestFromName(string name)
    {
        return new TerraformTemplate()
        {
            Name = name,
        };
    }

    public static TerraformTemplate GetTemplateByName(string name)
    {
        var templateName = name.ToLower();
        if (templateName.StartsWith("terraform:"))
        {
            templateName = templateName.Replace("terraform:", string.Empty);
        }

        return templateName switch
        {
            NewProjectTemplate => LatestFromName(NewProjectTemplate),
            AzureStorageBlob => LatestFromName(AzureStorageBlob),
            AzureDatabricks => LatestFromName(AzureDatabricks),
            AzureAppService => LatestFromName(AzureDatabricks),
            AzurePostgres => LatestFromName(AzurePostgres),
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }

    public static List<TerraformTemplate> LatestFromNameWithDependencies(string name)
    {
        return name switch
        {
            NewProjectTemplate =>
            [
                LatestFromName(NewProjectTemplate)
            ],
            AzureStorageBlob =>
            [
                LatestFromName(AzureStorageBlob)
            ],
            AzureDatabricks =>
            [
                LatestFromName(AzureStorageBlob),
                LatestFromName(AzureDatabricks)
            ],
            AzureAppService =>
            [
                LatestFromName(AzureStorageBlob),
                LatestFromName(AzureAppService)
            ],
            AzurePostgres =>
            [
                LatestFromName(AzurePostgres)
            ],
            VariableUpdate =>
            [
                LatestFromName(VariableUpdate)
            ],
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }
}