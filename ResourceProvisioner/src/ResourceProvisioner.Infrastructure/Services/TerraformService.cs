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

    public async Task CopyTemplateAsync(TerraformTemplate template, TerraformWorkspace terraformWorkspace)
    {
        if (template.Name is TerraformTemplate.VariableUpdate or TerraformTemplate.ContactUs)
        {
            return;
        }

        var templateSourcePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, template.Name);
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);

        _logger.LogInformation("Copying template from {ModuleSource} to {ProjectPath}", templateSourcePath,
            projectPath);

        if (!Directory.Exists(projectPath))
        {
            if (template.Name == TerraformTemplate.NewProjectTemplate)
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
            fileContent = fileContent.Replace(TerraformBranchToken, $"?ref={_resourceProvisionerConfiguration.ModuleRepository.Branch}");
            await File.WriteAllTextAsync(destinationFilename, fileContent);
        }
    }

    /// <summary>
    /// extracts the variables from the template and potential values from the workspace and or the configuration 
    /// and if the variable file already exists, then from there. And finally writes the variables to the variables file
    /// </summary>
    /// <param name="template"></param>
    /// <param name="terraformWorkspace"></param>
    /// <returns></returns>
    public async Task ExtractVariables(TerraformTemplate template, TerraformWorkspace terraformWorkspace)
    {
        if (template.Name is TerraformTemplate.VariableUpdate or TerraformTemplate.ContactUs)
        {
            return;
        }

        //Identify the variables marked as mandatory in the template and fill the values from the
        //workspace or configuration or an already existing variables file
        var missingVariables = FindMissingVariables(template, terraformWorkspace);

        //write or overwrite the variables file
        await WriteVariablesFile(template, terraformWorkspace, missingVariables);
    }

    /// <summary>
    /// Constructs the project.tfbackend file for the given acronym of the project 
    /// from the configuration and the workspace acronym using static logic
    /// </summary>
    /// <param name="workspaceAcronym"></param>
    /// <returns></returns>
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
            }
        };

        // Write the dictionary into a key value pair file
        await File.WriteAllLinesAsync(backendConfigFilePath, backendConfig.Select(x => $"{x.Key} = \"{x.Value}\""));
    }

    /// <summary>
    /// for each template of the workspace, reconstructs all the variable values required 
    /// by the template and writes it into the variables file for each template
    /// </summary>
    /// <param name="terraformWorkspace"></param>
    /// <returns></returns>
    /// <exception cref="ProjectNotInitializedException"></exception>
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

        // *.auto.tfvars.json are basically all the variable files for every template required for the workspace
        var files = Directory.GetFiles(projectPath, "*.auto.tfvars.json", SearchOption.TopDirectoryOnly);

        // the first part refers ti the template names
        var templateFileNames = files.Select(file => Path.GetFileName(file).Split('.')[0]).ToList();

        // match against the templates and rerun the extract variables
        foreach (var templateName in templateFileNames)
        {
            try
            {
                var templateObject = TerraformTemplate.GetTemplateByName(templateName);

                // for the given template object, extracts the variables from the template and potential values from the workspace and or the configuration 
                // and if the variable file already exists, then from there. And finally writes the variables to the variables file
                await ExtractVariables(templateObject, terraformWorkspace);
            }
            catch (ArgumentException)
            {
                _logger.LogWarning("Unable to find template for variable {VariableName}", templateName);
            }
        }
    }

    /// <summary>
    /// variables.tf.json contains all the required variables. This function searches all *.auto.tfvars.json and returns the 
    /// mandatory variables which are either missing or has no values in any of the *.auto.tfvars.json files
    /// </summary>
    /// <param name="template"></param>
    /// <param name="terraformWorkspace"></param>
    /// <returns></returns>
    private Dictionary<string, (string, bool)> FindMissingVariables(TerraformTemplate template,
        TerraformWorkspace terraformWorkspace)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        var templatePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, template.Name);

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
            .ToDictionary(jsonProperty => jsonProperty.Name, jsonProperty => (jsonProperty.Value.ValueKind.ToString(), true));
        return existingVariables;
    }

    private async Task WriteVariablesFile(TerraformTemplate template, TerraformWorkspace terraformWorkspace,
        Dictionary<string, (string Value, bool isRequired)> missingVariables)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);
        var variablesFilePath = Path.Join(projectPath, $"{template.Name}.auto.tfvars.json");

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
                        ComputeVariableValue(terraformWorkspace, missingVariable.Key, missingVariable.Value.Value, missingVariable.Value.isRequired)))
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
                $"{_resourceProvisionerConfiguration.Terraform.Variables.resource_prefix}{_resourceProvisionerConfiguration.Terraform.Variables.environment_name}terraformbackend",
            TerraformVariables.BackendContainerName =>
                $"{_resourceProvisionerConfiguration.Terraform.Variables.resource_prefix}-project-states",
            TerraformVariables.BackendKeyName =>
                $"{_resourceProvisionerConfiguration.Terraform.Variables.resource_prefix}-{workspaceName}.tfstate",
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

    public async Task DestroySpecificResourcesAsync(TerraformWorkspace terraformWorkspace, string resourceIdentifier)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, terraformWorkspace.Acronym);

        Directory.SetCurrentDirectory(projectPath);

        if (!await ExecuteShellCommand("terraform init")) {
            _logger.LogError("Terraform init failed.");
            return;
        }

        var destroyCmd = $"terraform destroy -auto-approve -target={resourceIdentifier}";
        if (!await ExecuteShellCommand(destroyCmd)) {
            _logger.LogError($"Terraform destroy failed for {resourceIdentifier}.");
        }

        _logger.LogInformation("Successfully destroyed specified resources.");
    }
    private async Task<bool> ExecuteShellCommand(string command)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }

}