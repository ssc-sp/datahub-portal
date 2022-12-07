using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Domain.Entities;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.Services;

public class TerraformService : ITerraformService
{
    private readonly ILogger<TerraformService> _logger;
    private readonly IConfiguration _configuration;

    public const string MapAnyType = "map(any)";
    public const string NewProjectTemplate = "new-project-template";
    
    private const string BackendResourceGroupName = "resource_group_name";
    private const string BackendStorageAccountName = "storage_account_name";
    private const string BackendContainerName = "container_name";
    private const string BackendKeyName = "key";

    public TerraformService(ILogger<TerraformService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task CopyTemplateAsync(DataHubTemplate template, string workspaceAcronym)
    {
        var templateSourcePath = DirectoryUtils.GetTemplatePath(_configuration, template.Name);
        var projectPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);

        _logger.LogInformation("Copying template from {ModuleSource} to {ProjectPath}", templateSourcePath,
            projectPath);

        if (!Directory.Exists(projectPath))
        {
            if (template.Name == NewProjectTemplate)
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

        var files = Directory.GetFiles(templateSourcePath, "*.*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            var sourceFilename = Path.GetFileName(file);
            var destinationFilename = Path.Combine(projectPath, sourceFilename);
            File.Copy(file, destinationFilename, true);
        }

        return Task.CompletedTask;
    }

    public async Task ExtractVariables(DataHubTemplate template, string workspaceAcronym)
    {
        var missingVariables = FindMissingVariables(template, workspaceAcronym);
        await WriteVariablesFile(template, workspaceAcronym, missingVariables);
    }
    
    public async Task ExtractBackendConfig(string workspaceAcronym)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);
        var backendConfigFilePath = Path.Join(projectPath, "project.tfbackend");

        if (File.Exists(backendConfigFilePath))
            return;

        var backendConfig = new Dictionary<string, string>
        {
            { BackendResourceGroupName, ComputeBackendConfigValue(workspaceAcronym, BackendResourceGroupName) },
            { BackendStorageAccountName, ComputeBackendConfigValue(workspaceAcronym, BackendStorageAccountName) },
            { BackendContainerName, ComputeBackendConfigValue(workspaceAcronym, BackendContainerName) },
            { BackendKeyName, ComputeBackendConfigValue(workspaceAcronym, BackendKeyName) }
        };
        
        // Write the dictionary into a key value pair file
        await File.WriteAllLinesAsync(backendConfigFilePath, backendConfig.Select(x => $"{x.Key} = \"{x.Value}\""));
    }

    private Dictionary<string, string> FindMissingVariables(DataHubTemplate template, string workspaceAcronym)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);
        var templatePath = DirectoryUtils.GetTemplatePath(_configuration, template.Name);

        var existingVariables = FindExistingVariables(projectPath);
        var requiredTemplateVariables = GetRequiredTemplateVariables(templatePath);

        var missingVariables = requiredTemplateVariables.Except(existingVariables)
            .ToDictionary(x => x.Key, x => x.Value);

        return missingVariables;
    }

    private static Dictionary<string, string> FindExistingVariables(string projectPath)
    {
        var files = Directory.GetFiles(projectPath, "*.auto.tfvars.json", SearchOption.TopDirectoryOnly);
        var existingVariables = files.Select(file => JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(file)))
            .SelectMany(jsonNode => jsonNode.EnumerateObject())
            .ToDictionary(jsonProperty => jsonProperty.Name, jsonProperty => jsonProperty.Value.ValueKind.ToString());
        return existingVariables;
    }

    private async Task WriteVariablesFile(DataHubTemplate template, string workspaceAcronym,
        Dictionary<string, string> missingVariables)
    {
        var projectPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);
        var variablesFilePath = Path.Join(projectPath, $"{template.Name}.auto.tfvars.json");

        if (File.Exists(variablesFilePath))
        {
            var preExistingVariables =
                JsonSerializer.Deserialize<JsonObject>(
                    await File.ReadAllTextAsync(variablesFilePath)) ?? new JsonObject();
            foreach (var (key, value) in missingVariables)
            {
                preExistingVariables.TryAdd(key, ComputeVariableValue(workspaceAcronym, key, value));
            }

            await File.WriteAllTextAsync(variablesFilePath, JsonSerializer.Serialize(preExistingVariables));
        }
        else
        {
            await File.WriteAllTextAsync(variablesFilePath,
                JsonSerializer.Serialize(missingVariables.ToDictionary(mv => mv.Key,
                    mv => ComputeVariableValue(workspaceAcronym, mv.Key, mv.Value))));
        }
    }

    private JsonNode ComputeVariableValue(string workspaceAcronym, string variableName, string variableType)
    {
        if(variableType == MapAnyType)
        {
            return ComputeMapVariableValue(variableName);
        }
        
        var configValue = _configuration[$"Terraform:Variables:{variableName}"];
        if (!string.IsNullOrEmpty(configValue))
            return configValue!;

        return (variableName switch
        {
            "project_cd" => workspaceAcronym,
            _ => throw new MissingTerraformVariableException(
                $"Missing variable {variableName}:<{variableType}> in configuration")
        })!;
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
        return (variableName switch
        {
            BackendResourceGroupName => _configuration["Terraform:Backend:ResourceGroupName"],
            BackendStorageAccountName => $"{_configuration["Terraform:Variables:resource_prefix"]}{_configuration["Terraform:Variables:environment_name"]}terraformbackend",
            BackendContainerName => $"{_configuration["Terraform:Variables:resource_prefix"]}-project-states",
            BackendKeyName => $"{_configuration["Terraform:Variables:resource_prefix"]}-{workspaceName}.tfstate",
            _ => throw new MissingTerraformVariableException(
                $"Missing variable {variableName}:<string> in configuration")
        })!;
    }

    public Dictionary<string, string> GetRequiredTemplateVariables(string templatePath)
    {
        var requiredVariables = new Dictionary<string, string>();

        Directory.GetFiles(templatePath, "*.variables.tf.json", SearchOption.TopDirectoryOnly)
            .Select(file => ParseVariableDefinitions(File.ReadAllText(file)))
            .ToList()
            .ForEach(values =>
                values.ToList()
                    .ForEach(val => requiredVariables.Add(val.Key, val.Value)));

        return requiredVariables;
    }


    public static Dictionary<string, string> ParseVariableDefinitions(string variableJson)
    {
        var propertiesJson = JsonSerializer.Deserialize<JsonObject>(variableJson);

        return propertiesJson?["variable"]?.AsObject().ToDictionary(
            property => property.Key,
            property => property.Value?["type"]?.ToString() ?? ""
        ) ?? new Dictionary<string, string>();
    }
}