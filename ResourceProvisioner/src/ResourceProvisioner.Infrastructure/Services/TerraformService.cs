using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.Services;

public class TerraformService : ITerraformService
{
    public const string TerraformVersionToken = "{{version}}";
    public const string TerraformBranchToken = "{{branch}}";

    private readonly ILogger<TerraformService> _logger;
    internal static readonly List<string> EXCLUDED_FILE_EXTENSIONS = new(new[] { ".md" });
    private readonly ResourceProvisionerConfiguration _resourceProvisionerConfiguration;
    private readonly IConfiguration _configuration;

    public TerraformService(ILogger<TerraformService> logger,
        ResourceProvisionerConfiguration resourceProvisionerConfiguration, IConfiguration configuration)
    {
        _logger = logger;
        _resourceProvisionerConfiguration = resourceProvisionerConfiguration;
        _configuration = configuration;
    }

    public async Task CopyTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace)
    {
        if (templateName is TerraformTemplate.VariableUpdate or TerraformTemplate.ContactUs)
        {
            return;
        }

        var templateSourcePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, templateName);
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);

        _logger.LogInformation("Copying template from {ModuleSource} to {ProjectPath}", templateSourcePath,
            projectPath);

        if (!Directory.Exists(projectPath))
        {
            if (templateName == TerraformTemplate.NewProjectTemplate)
            {
                _logger.LogInformation("Creating new project directory {ProjectPath}", projectPath);
                Directory.CreateDirectory(projectPath);
            }
            else
            {
                _logger.LogInformation(
                    "Project directory {ProjectPath} does not exist please run template module first", projectPath);
                throw new ProjectNotInitializedException(
                    "Project directory does not exist please run template module first");
            }
        }


        var files = Directory.GetFiles(templateSourcePath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(filename => !EXCLUDED_FILE_EXTENSIONS.Contains(Path.GetExtension(filename)));

        foreach (var file in files)
        {
            var sourceFilename = Path.GetFileName(file);
            var destinationFilename = Path.Combine(projectPath, sourceFilename);

            var fileContent = await File.ReadAllTextAsync(file);
            fileContent = fileContent.Replace(TerraformVersionToken, terraformWorkspace.Version);
            fileContent = fileContent.Replace(TerraformBranchToken,
                $"?ref={_resourceProvisionerConfiguration.ModuleRepository.Branch}");
            await File.WriteAllTextAsync(destinationFilename, fileContent);
        }
    }

    public async Task ExtractVariables(string templateName, TerraformWorkspace terraformWorkspace)
    {
        if (templateName is TerraformTemplate.VariableUpdate or TerraformTemplate.ContactUs)
        {
            return;
        }

        var missingVariables = FindMissingVariables(templateName, terraformWorkspace);
        await WriteVariablesFile(templateName, terraformWorkspace, missingVariables);
    }

    public async Task ExtractBackendConfig(string workspaceAcronym)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
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

    public async Task ExtractAllVariables(TerraformWorkspace terraformWorkspace)
    {
        // check if the project directory exists
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        if (!Directory.Exists(projectPath))
        {
            _logger.LogInformation(
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
                await ExtractVariables(template, terraformWorkspace);
            }
            catch (ArgumentException)
            {
                _logger.LogWarning("Unable to find template for variable {VariableName}", variableName);
            }
        }
    }

    public async Task DeleteTemplateAsync(string templateName, TerraformWorkspace terraformWorkspace)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        
        var matchingFiles = Directory.GetFiles(projectPath, $"{templateName}.*");
        if(matchingFiles.Length > 0)
        {
            foreach (var file in matchingFiles)
            {
                File.Delete(file);
                _logger.LogInformation("Deleted file {File}", file);
            }
        }
        
        await WriteDeletedFile(templateName, projectPath);
    }
    
    private async Task WriteDeletedFile(string templateName, string projectPath)
    {
        var deletedFilePath = Path.Join(projectPath, $"{templateName}.tf");
        var content = $"output \"{_terraformModuleStatusOutputName[templateName]}\" {{\n  value = \"deleted\"\n}}";
        await File.WriteAllTextAsync(deletedFilePath, content);
        _logger.LogInformation("Created deleted file {DeletedFilePath}", deletedFilePath);
    }
    
    private readonly Dictionary<string, string> _terraformModuleStatusOutputName = new()
    {
        {TerraformTemplate.AzureStorageBlob, "azure_storage_blob_status"},
        {TerraformTemplate.AzureDatabricks, "azure_databricks_module_status"},
        // {TerraformTemplate.AzureVirtualMachine, "public_ip_address"},
        {TerraformTemplate.AzureAppService, "azure_app_service_module_status"},
        {TerraformTemplate.AzurePostgres, "azure_psql_module_status"},
        // {TerraformTemplate.AzureArcGis, "arcgis_url"},
        // {TerraformTemplate.AzureAPI, "api_url"}
    };

    private Dictionary<string, (string, bool)> FindMissingVariables(string templateName,
        TerraformWorkspace terraformWorkspace)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        var templatePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, templateName);

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

    private async Task WriteVariablesFile(string templateName, TerraformWorkspace terraformWorkspace,
        Dictionary<string, (string Value, bool isRequired)> missingVariables)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        var variablesFilePath = Path.Join(projectPath, $"{templateName}.auto.tfvars.json");

        if (File.Exists(variablesFilePath))
        {
            var preExistingVariables =
                JsonSerializer.Deserialize<JsonObject>(
                    await File.ReadAllTextAsync(variablesFilePath)) ?? new JsonObject();
            foreach (var (key, (value, isRequired)) in missingVariables)
            {
                preExistingVariables.Remove(key);
                var variableValue = ComputeVariableValue(terraformWorkspace, key, value, isRequired);
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
                        ComputeVariableValue(terraformWorkspace, missingVariable.Key, missingVariable.Value.Value,
                            missingVariable.Value.isRequired)))
                    .Where(mv => mv.Item2 != null)
                    .ToDictionary(mv => mv.Key, mv => mv.Item2))
            );
        }
    }

    // ReSharper disable once ReturnTypeCanBeNotNullable
    // This can return null if the variable is not required
    private JsonNode? ComputeVariableValue(TerraformWorkspace terraformWorkspace, string variableName,
        string variableType, bool isRequired = false)
    {
        if (variableType.StartsWith(TerraformVariables.MapType, StringComparison.InvariantCultureIgnoreCase))
        {
            return ComputeMapVariableValue(variableName);
        }

        if (variableType == TerraformVariables.ListAnyType)
        {
            return ComputeListVariableValue(terraformWorkspace, variableName);
        }

        var configValue = _configuration[$"Terraform:Variables:{variableName}"];
        if (!string.IsNullOrEmpty(configValue))
            return configValue!;

        return (variableName switch
        {
            TerraformVariables.ProjectAcronym => terraformWorkspace.Acronym,
            TerraformVariables.BudgetAmount => terraformWorkspace.BudgetAmount,
            TerraformVariables.StorageSizeLimitInTb => terraformWorkspace.StorageSizeLimitInTB,

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
        var configValue = _configuration
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
            TerraformVariables.BackendResourceGroupName => _resourceProvisionerConfiguration.Terraform.Backend
                .ResourceGroupName,
            TerraformVariables.BackendStorageAccountName =>
                $"{_resourceProvisionerConfiguration.Terraform.Variables.resource_prefix_alphanumeric}{_resourceProvisionerConfiguration.Terraform.Variables.environment_name}{_resourceProvisionerConfiguration.Terraform.Variables.storage_suffix}",
            TerraformVariables.BackendContainerName =>
                $"{_resourceProvisionerConfiguration.Terraform.Variables.resource_prefix}-project-states",
            TerraformVariables.BackendKeyName =>
                $"{_resourceProvisionerConfiguration.Terraform.Variables.resource_prefix}-{workspaceName}.tfstate",
            TerraformVariables.BackendSubscriptionIdName =>
                $"{_resourceProvisionerConfiguration.Terraform.Variables.az_subscription_id}",
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