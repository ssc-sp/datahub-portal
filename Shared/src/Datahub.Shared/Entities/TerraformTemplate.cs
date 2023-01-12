using System;
using System.Collections.Generic;

namespace Datahub.Shared.Entities;

public class TerraformTemplate
{
    public const string NewProjectTemplate = "new-project-template";
    public const string VariableUpdate = "variable-update";
    public const string AzureStorageBlob = "azure-storage-blob";
    public const string AzureDatabricks = "azure-databricks";
    public const string ContactUs = "contact-us";

    public string Name { get; set; }
    public string Version { get; set; }

    public static TerraformTemplate Default => LatestFromName(NewProjectTemplate);

    public static TerraformTemplate LatestFromName(string name)
    {
        return new TerraformTemplate()
        {
            Name = name,
            Version = "latest",
        };
    }
    
    public static List<TerraformTemplate> LatestFromNameWithDependencies(string name)
    {
        return name switch
        {
            NewProjectTemplate => new List<TerraformTemplate>()
            {
                LatestFromName(NewProjectTemplate),
            },
            AzureStorageBlob => new List<TerraformTemplate>()
            {
                LatestFromName(AzureStorageBlob),
            },
            AzureDatabricks => new List<TerraformTemplate>()
            {
                LatestFromName(AzureStorageBlob),
                LatestFromName(AzureDatabricks),
            },
            VariableUpdate => new List<TerraformTemplate>()
            {
                LatestFromName(VariableUpdate),
            },
            _ => throw new ArgumentException($"Unknown template name: {name}")
        };
    }
}