using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Datahub.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.Services;

public class TerraformService(
    ILogger<TerraformService> logger,
    ResourceProvisionerConfiguration resourceProvisionerConfiguration,
    IConfiguration configuration)
    : ITerraformService
{
    //public const string TerraformVersionToken = "{{version}}";
    //public const string TerraformBranchToken = "{{branch}}";
    public const string TerraformTagToken = "{{tag}}";
    public const string TerraformVersionToken = "{{version}}";

    internal static readonly List<string> EXCLUDED_FILE_EXTENSIONS = new(new[] { ".md" });

    public async Task CopyTemplateAsync(string templateName, CreateResourceRunCommand workspaceDefinition)
    {
        if (templateName is TerraformTemplate.VariableUpdate or TerraformTemplate.ContactUs)
        {
            return;
        }

        var templateSourcePath = DirectoryUtils.GetTemplatePath(resourceProvisionerConfiguration, templateName);
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, workspaceDefinition.Workspace.Acronym);

        logger.LogInformation("Copying template from {ModuleSource} to {ProjectPath}", templateSourcePath,
            projectPath);

        if (!Directory.Exists(projectPath))
        {
            if (templateName == TerraformTemplate.NewProjectTemplate)
            {
                logger.LogInformation("Creating new project directory {ProjectPath}", projectPath);
                Directory.CreateDirectory(projectPath);
            }
            else
            {
                logger.LogInformation(
                    "Project directory {ProjectPath} does not exist please run template module first", projectPath);
                throw new ProjectNotInitializedException(
                    "Project directory does not exist please run template module first");
            }
        }

        await DeletedAnyOldDeletedFiles(projectPath, templateName);

        var files = Directory.GetFiles(templateSourcePath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(filename => !EXCLUDED_FILE_EXTENSIONS.Contains(Path.GetExtension(filename)));

        foreach (var file in files)
        {
            var sourceFilename = Path.GetFileName(file);
            var destinationFilename = Path.Combine(projectPath, sourceFilename);

            var fileContent = await File.ReadAllTextAsync(file);
            
            fileContent = fileContent.Replace(TerraformTagToken,
                $"?ref={resourceProvisionerConfiguration.ModuleRepository.Branch}-{workspaceDefinition.Workspace.Version}");
            fileContent = fileContent.Replace(TerraformVersionToken, workspaceDefinition.Workspace.Version);

            await File.WriteAllTextAsync(destinationFilename, fileContent);
        }
    }

    public async Task ExtractVariables(string templateName, CreateResourceRunCommand command)
    {
        if (templateName is TerraformTemplate.VariableUpdate or TerraformTemplate.ContactUs)
        {
            return;
        }

        var missingVariables = FindMissingVariables(templateName, command.Workspace);
        await WriteVariablesFile(templateName, command.Workspace, missingVariables, command.AppData);
    }

    public async Task ExtractBackendConfig(string workspaceAcronym)
    {
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, workspaceAcronym);
        var backendConfigFilePath = Path.Join(projectPath, "project.tfbackend");

        if (File.Exists(backendConfigFilePath))
            return;

        var backendConfig = new Dictionary<string, string>
        {
            {
                TerraformVariables.BackendResourceGroupName,
                ComputeBackendConfigValue(workspaceAcronym, TerraformVariables.BackendResourceGroupName)
            },
            {
                TerraformVariables.BackendStorageAccountName,
                ComputeBackendConfigValue(workspaceAcronym, TerraformVariables.BackendStorageAccountName)
            },
            {
                TerraformVariables.BackendContainerName,
                ComputeBackendConfigValue(workspaceAcronym, TerraformVariables.BackendContainerName)
            },
            {
                TerraformVariables.BackendKeyName,
                ComputeBackendConfigValue(workspaceAcronym, TerraformVariables.BackendKeyName)
            },
            {
                TerraformVariables.BackendSubscriptionIdName,
                ComputeBackendConfigValue(workspaceAcronym, TerraformVariables.BackendSubscriptionIdName)
            }
        };

        // Write the dictionary into a key value pair file
        await File.WriteAllLinesAsync(backendConfigFilePath, backendConfig.Select(x => $"{x.Key} = \"{x.Value}\""));
    }

    public async Task ExtractAllVariables(CreateResourceRunCommand command)
    {
        // check if the project directory exists
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, command.Workspace.Acronym);
        if (!Directory.Exists(projectPath))
        {
            logger.LogInformation(
                "Project directory {ProjectPath} does not exist please run template module first", projectPath);
            throw new ProjectNotInitializedException(
                "Project directory does not exist please run template module first");
        }

        // get all the files in the project directory that end with *.auto.tfvars.json
        var files = Directory.GetFiles(projectPath, "*.auto.tfvars.json", SearchOption.TopDirectoryOnly);

        // get the filename up to the first "."
        var variableNames = files.Select(file => Path.GetFileName(file).Split('.')[0]).ToList();

        // match against the templates and rerun the extract variables
        foreach (var variableName in variableNames)
        {
            try
            {
                var template = TerraformTemplate.NormalizeTemplateName(variableName);
                await ExtractVariables(template, command);
            }
            catch (ArgumentException)
            {
                logger.LogWarning("Unable to find template for variable {VariableName}", variableName);
            }
        }
    }

    public async Task DeleteWorkspaceAsync(TerraformWorkspace terraformWorkspace, string resourcegroup)
    {
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        await RenameTemplateAsDeleted(projectPath, TerraformTemplate.NewProjectTemplateFile, terraformWorkspace);

        var workspaceDeletionFilePath = Path.Join(projectPath, $"delete.ps1");

        var contentline1 = $"az account set --subscription {terraformWorkspace.SubscriptionId}";
        var contentline2 = $"az group delete --name {resourcegroup} --yes";

        var content = $"{contentline1}\n{contentline2}";
        await File.WriteAllTextAsync(workspaceDeletionFilePath, content);
        logger.LogInformation("Created workspace deletion file {DeletedFilePath}", workspaceDeletionFilePath);
    }

    public async Task DeleteTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace)
    {
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        templateName = templateName == TerraformTemplate.AzurePostgres ? TerraformTemplate.AzurePostgresTemplateFile : templateName;
        await RenameTemplateAsDeleted(projectPath, templateName, terraformWorkspace);
        await WriteDeletedFile(templateName, projectPath);
    }

    private async Task RenameTemplateAsDeleted(string projectPath, string templateName, TerraformWorkspace terraformWorkspace)
    {
        // check if the project directory exists
        await DeletedAnyOldDeletedFiles(projectPath, templateName);

        var matchingFiles = Directory.GetFiles(projectPath, $"{templateName}.tf", SearchOption.AllDirectories)
            .ToArray();

        if (matchingFiles.Length > 0)
        {
            foreach (var file in matchingFiles)
            {
                var newFileName = $"{file}.deleted";
                File.Move(file, newFileName);
                logger.LogInformation("Renamed file {File} to {NewFileName}", file, newFileName);
            }
        }
    }

    private async Task DeletedAnyOldDeletedFiles(string projectPath, string templateName)
    {        
        var matchingFiles = Directory.GetFiles(projectPath, $"{templateName}.tf.deleted", SearchOption.AllDirectories)
            .ToArray();

        if (matchingFiles.Length > 0)
        {
            foreach (var file in matchingFiles)
            {
                File.Delete(file);
                logger.LogInformation("Deleted file {DeletedFilePath}", file);
            }
        }
    }

    public virtual async Task WriteDeletedFile(string templateName, string projectPath)
    {
        var deletedFilePath = Path.Join(projectPath, $"{templateName}.tf");
        var content = $"output \"{TerraformModuleStatusOutputName[templateName]}\" {{\n  value = \"deleted\"\n}}";
        await File.WriteAllTextAsync(deletedFilePath, content);
        logger.LogInformation("Created deleted file {DeletedFilePath}", deletedFilePath);
    }
    
    internal static readonly Dictionary<string, string> TerraformModuleStatusOutputName = new()
    {
        {TerraformTemplate.AzureStorageBlob, TerraformVariables.OutputAzureStorageBlobStatus},
        {TerraformTemplate.AzureDatabricks, TerraformVariables.OutputAzureDatabricksStatus},
        // {TerraformTemplate.AzureVirtualMachine, "public_ip_address"},
        {TerraformTemplate.AzureAppService, TerraformVariables.OutputAzureAppServiceStatus},
        {TerraformTemplate.AzurePostgres, TerraformVariables.OutputAzurePostgresStatus},
        {TerraformTemplate.AzurePostgresTemplateFile, TerraformVariables.OutputAzurePostgresStatus},
        {TerraformTemplate.NewProjectTemplate, TerraformVariables.OutputNewProjectTemplate}
        // {TerraformTemplate.AzureArcGis, "arcgis_url"},
        // {TerraformTemplate.AzureAPI, "api_url"}
    };

    /// <summary>
    /// Missing Variables are those that are defined in the template but not yet defined in the project
    /// </summary>
    /// <param name="templateName"></param>
    /// <param name="terraformWorkspace"></param>
    /// <returns>variable name and type</returns>
    private Dictionary<string, (string, bool)> FindMissingVariables(string templateName,
        TerraformWorkspace terraformWorkspace)
    {
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        var templatePath = DirectoryUtils.GetTemplatePath(resourceProvisionerConfiguration, templateName);

        var existingVariables = FindExistingVariables(projectPath);
        var requiredTemplateVariables = GetRequiredTemplateVariables(templatePath);

        var missingVariables = requiredTemplateVariables.Except(existingVariables)
            .ToDictionary(x => x.Key, x => x.Value);

        return missingVariables;
    }

    private static Dictionary<string, (string, bool)> FindExistingVariables(string projectPath)
    {
        var files = Directory.GetFiles(projectPath, "*.auto.tfvars.json", SearchOption.TopDirectoryOnly);
        var existingVariables = files.Select(file => JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(file)))
            .SelectMany(jsonNode => jsonNode.EnumerateObject())
            .ToDictionary(jsonProperty => jsonProperty.Name,
                jsonProperty => (jsonProperty.Value.ValueKind.ToString(), true));
        return existingVariables;
    }

    /// <summary>
    /// Creates or updates a Terraform variables file for the specified template and workspace, ensuring that all
    /// required variables are present and up to date.
    /// </summary>
    /// <remarks>If a variables file already exists, only the specified variables are updated or added; all
    /// other existing variables are preserved. If the file does not exist, it is created with only the provided
    /// variables. The method overwrites the file in both cases.</remarks>
    /// <param name="templateName">The name of the Terraform template for which the variables file is generated. This determines the filename of
    /// the variables file.</param>
    /// <param name="terraformWorkspace">The workspace context containing environment and configuration details used to resolve variable values.</param>
    /// <param name="missingVariables">A dictionary of variable names and their associated values and required status. Each entry represents a variable
    /// that must be written or updated in the variables file.</param>
    /// <param name="workspaceAppData">Application-specific data for the workspace, used to compute or resolve variable values as needed.</param>
    /// <returns>A task that represents the asynchronous operation of writing the variables file.</returns>
    private async Task WriteVariablesFile(string templateName, TerraformWorkspace terraformWorkspace,
        Dictionary<string, (string Value, bool isRequired)> missingVariables, WorkspaceAppData workspaceAppData)
    {
        var projectPath = DirectoryUtils.GetProjectPath(resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        var variablesFilePath = Path.Join(projectPath, $"{templateName}.auto.tfvars.json");

        if (File.Exists(variablesFilePath))
        {
            var preExistingVariables =
                JsonSerializer.Deserialize<JsonObject>(
                    await File.ReadAllTextAsync(variablesFilePath)) ?? new JsonObject();
            foreach (var (key, (value, isRequired)) in missingVariables)
            {
                preExistingVariables.Remove(key);
                var variableValue = ComputeVariableValue(terraformWorkspace, workspaceAppData, key, value, templateName, isRequired);
                if (variableValue != null)
                {
                    preExistingVariables.TryAdd(key, variableValue);
                }
            }

            await File.WriteAllTextAsync(variablesFilePath, JsonSerializer.Serialize(preExistingVariables));
        }
        else
        {
            await File.WriteAllTextAsync(variablesFilePath,
                JsonSerializer.Serialize(missingVariables
                    .Select(missingVariable => (
                        missingVariable.Key,
                        ComputeVariableValue(terraformWorkspace, workspaceAppData, missingVariable.Key, missingVariable.Value.Value, templateName,
                            missingVariable.Value.isRequired)))
                    .Where(mv => mv.Item2 != null)
                    .ToDictionary(mv => mv.Key, mv => mv.Item2))
            );
        }
    }

    // ReSharper disable once ReturnTypeCanBeNotNullable
    // This can return null if the variable is not required
    private JsonNode? ComputeVariableValue(TerraformWorkspace terraformWorkspace, WorkspaceAppData workspaceAppData, string variableName,
        string variableType, string templateName, bool isRequired = false)
    {
        if (variableType.StartsWith(TerraformVariables.MapType, StringComparison.InvariantCultureIgnoreCase))
        {
            return ComputeMapVariableValue(variableName);
        }

        if (variableType == TerraformVariables.ListAnyType)
        {
            return ComputeListVariableValue(terraformWorkspace, variableName);
        }

        var configValue = configuration[$"Terraform:Variables:{variableName}"];
        if (!string.IsNullOrEmpty(configValue))
            return configValue!;

        return (variableName switch
        {
            TerraformVariables.ProjectAcronym => terraformWorkspace.Acronym,
            TerraformVariables.BudgetAmount => terraformWorkspace.BudgetAmount,
            TerraformVariables.StorageSizeLimitInTb => terraformWorkspace.StorageSizeLimitInTB,
            TerraformVariables.PsqlSku => workspaceAppData.PostgresConfiguration?.PSQL_SKU ?? "B_Standard_B1ms",
            TerraformVariables.PsqlNameSuffix => workspaceAppData.PostgresConfiguration?.ResourceNameSuffix ?? string.Empty,
            TerraformVariables.AppServiceNameSuffix => workspaceAppData.AppServiceConfiguration?.ResourceNameSuffix ?? string.Empty,
            TerraformVariables.EnableMlCluster => workspaceAppData.DatabricksConfiguration?.EnableMachineLearning ?? false,
            TerraformVariables.EnableMlGpuCluster => workspaceAppData.DatabricksConfiguration?.EnableMachineLearningGpu ?? false,
            TerraformVariables.MlCompute => workspaceAppData.DatabricksConfiguration?.MachineLearningTierSku ?? DatabricksTier.DefaultML.DatabricksSKU,
            TerraformVariables.MlGpuCompute => workspaceAppData.DatabricksConfiguration?.MachineLearningGpuTierSku ?? DatabricksTier.DefaultMLGpu.DatabricksSKU,
            TerraformVariables.SSCCBRID => terraformWorkspace.SSCCBRID,
            // optional variables
            TerraformVariables.AzureLogWorkspaceId => string.Empty,
            TerraformVariables.AllowSourceIp => string.Empty,
            _ => isRequired
                ? throw new MissingTerraformVariableException(
                    $"Missing variable {variableName}:<{variableType}> in configuration")
                : null
        })!;
    }

    private JsonNode ComputeListVariableValue(TerraformWorkspace terraformWorkspace, string variableName)
    {
        return variableName switch
        {
            TerraformVariables.DatabricksProjectLeadUsers => terraformWorkspace.ToUserList(Role.Owner),
            TerraformVariables.DatabricksAdminUsers => terraformWorkspace.ToUserList(Role.Admin),
            TerraformVariables.DatabricksProjectUsers => terraformWorkspace.ToUserList(Role.User),
            TerraformVariables.DatabricksProjectGuests => terraformWorkspace.ToUserList(Role.Guest),
            TerraformVariables.StorageContributorUsers => terraformWorkspace.ToUserList(new List<Role>()
            {
                Role.Owner, Role.Admin, Role.User
            }),
            TerraformVariables.StorageGuestUsers => terraformWorkspace.ToUserList(Role.Guest),
            _ => throw new MissingTerraformVariableException(
                $"Missing variable {variableName}:<{TerraformVariables.ListAnyType}> in configuration")
        };
    }


    private JsonObject ComputeMapVariableValue(string variableName)
    {
        var configValue = configuration
            .GetSection($"Terraform:Variables:{variableName}")
            .GetChildren()
            .Select(s => new KeyValuePair<string, JsonNode>(s.Key, (s.Value ?? string.Empty)!))
            .ToList();


        if (configValue.Any())
            return new JsonObject(configValue!);

        return variableName switch
        {
            _ => throw new MissingTerraformVariableException(
                $"Missing variable {variableName}:<Map> in configuration")
        };
    }

    private string ComputeBackendConfigValue(string workspaceName, string variableName)
    {
        return variableName switch
        {
            TerraformVariables.BackendResourceGroupName => resourceProvisionerConfiguration.Terraform.Backend
                .ResourceGroupName,
            TerraformVariables.BackendStorageAccountName =>
                $"{resourceProvisionerConfiguration.Terraform.Variables.resource_prefix_alphanumeric}{resourceProvisionerConfiguration.Terraform.Variables.environment_name}{resourceProvisionerConfiguration.Terraform.Variables.storage_suffix}",
            TerraformVariables.BackendContainerName =>
                $"{resourceProvisionerConfiguration.Terraform.Variables.resource_prefix}-project-states",
            TerraformVariables.BackendKeyName =>
                $"{resourceProvisionerConfiguration.Terraform.Variables.resource_prefix}-{workspaceName}.tfstate",
            TerraformVariables.BackendSubscriptionIdName =>
                $"{resourceProvisionerConfiguration.Terraform.Backend.SubscriptionId}",
            _ => throw new MissingTerraformVariableException(
                $"Missing variable {variableName}:<string> in configuration")
        };
    }

    private Dictionary<string, (string, bool)> GetRequiredTemplateVariables(string templatePath)
    {
        var requiredVariables = new Dictionary<string, (string, bool)>();

        Directory.GetFiles(templatePath, "*.variables.tf.json", SearchOption.TopDirectoryOnly)
            .Select(file => ParseVariableDefinitions(File.ReadAllText(file)))
            .ToList()
            .ForEach(values =>
                values.ToList()
                    .ForEach(val => requiredVariables.Add(val.Key, val.Value)));

        return requiredVariables;
    }


    public static Dictionary<string, (string, bool)> ParseVariableDefinitions(string variableJson)
    {
        var propertiesJson = JsonSerializer.Deserialize<JsonObject>(variableJson);

        return propertiesJson?["variable"]?
            .AsObject()
            .ToDictionary(
                property => property.Key,
                property => (property.Value?["type"]?.ToString() ?? "", property.Value?["default"]?.ToString() == null)
            ) ?? new Dictionary<string, (string, bool)>();
    }

    
}
