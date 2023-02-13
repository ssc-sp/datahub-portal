using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class AzureDatabricksTemplateTests
{
    [SetUp]
    public void RunBeforeEachTest()
    {
        var localModuleClonePath = DirectoryUtils.GetModuleRepositoryPath(_configuration);
        var localInfrastructureClonePath = DirectoryUtils.GetInfrastructureRepositoryPath(_configuration);

        VerifyDirectoryDoesNotExist(localModuleClonePath);
        VerifyDirectoryDoesNotExist(localInfrastructureClonePath);
    }

    [Test]
    public async Task ShouldThrowExceptionIfProjectNotInitialized()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        var module = new TerraformTemplate
        {
            Name = TerraformTemplate.AzureDatabricks,
            Version = "latest"
        };

        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(module, workspace);
        });
    }

    [Test]
    public async Task ShouldCopyAzureDatabricksTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var newProjectTemplateExpectedFileCount = await SetupNewProjectTemplate(workspaceAcronym);
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);

        await _terraformService.CopyTemplateAsync(module, workspace);

        var moduleSourcePath = DirectoryUtils.GetTemplatePath(_configuration, TerraformTemplate.AzureDatabricks);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);

        // verify all the files are copied except for the datahub readme
        var expectedFiles = Directory.GetFiles(moduleSourcePath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(filename => !TerraformService.EXCLUDED_FILE_EXTENSIONS.Contains(Path.GetExtension(filename)))
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
            Assert.That(Directory.GetFiles(moduleDestinationPath),
                Has.Length.EqualTo(expectedFiles.Count + newProjectTemplateExpectedFileCount));
        });

        // go through each file and assert that the content is the same
        foreach (var file in expectedFiles)
        {
            var sourceFileContent = await File.ReadAllTextAsync(file);
            var destinationFileContent =
                await File.ReadAllTextAsync(Path.Join(moduleDestinationPath, Path.GetFileName(file)));
            Assert.That(sourceFileContent, Is.EqualTo(destinationFileContent));
        }
    }

    [Test]
    public async Task ShouldExtractAzureDatabricksTemplateVariables()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);
        
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);
        var expectedVariables = GenerateExpectedVariables();

        await _terraformService.CopyTemplateAsync(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym),
            $"{module.Name}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables =
            JsonSerializer.Deserialize<JsonObject>(
                await File.ReadAllTextAsync(expectedVariablesFilename));

        foreach (var (key, value) in actualVariables!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(expectedVariables.ContainsKey(key), Is.True);
                Assert.That(value?.ToJsonString(), Is.EqualTo(expectedVariables[key]?.ToJsonString()));
            });
        }
    }



    [Test]
    public async Task ShouldExtractAzureDatabricksTemplateVariablesWithNoUsers()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);
        
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var expectedVariables = GenerateExpectedVariables(false);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);

        await _terraformService.CopyTemplateAsync(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym),
            $"{module.Name}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables =
            JsonSerializer.Deserialize<JsonObject>(
                await File.ReadAllTextAsync(expectedVariablesFilename));

        foreach (var (key, value) in actualVariables!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(expectedVariables.ContainsKey(key), Is.True);
                Assert.That(value?.ToJsonString(), Is.EqualTo(expectedVariables[key]?.ToJsonString()));
            });
        }
    }

    [Test]
    public async Task ShouldExtractAzureDatabricksTemplateVariablesWithoutDuplicates()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);

        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym);
        var expectedVariables = GenerateExpectedVariables();
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);

        await _terraformService.CopyTemplateAsync(module, workspace);

        await _terraformService.ExtractVariables(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym),
            $"{module.Name}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables =
            JsonSerializer.Deserialize<JsonObject>(
                await File.ReadAllTextAsync(expectedVariablesFilename));

        foreach (var (key, value) in actualVariables!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(expectedVariables.ContainsKey(key), Is.True);
                Assert.That(value?.ToJsonString(), Is.EqualTo(expectedVariables[key]?.ToJsonString()));
            });
        }
    }



    private static JsonObject GenerateExpectedVariables(bool withUsers = true)
    {
        if (!withUsers)
        {
            return new JsonObject
            {
                ["databricks_admin_users"] = new JsonArray(),
                ["azure_databricks_enterprise_oid"] = _resourceProvisionerConfiguration.Terraform.Variables
                    .azure_databricks_enterprise_oid
            };
        }

        return new JsonObject
        {
            ["databricks_admin_users"] = new JsonArray
            {
                new JsonObject
                {
                    ["email"] = "1@email.com",
                    ["oid"] = "00000000-0000-0000-0000-000000000001"
                },
                new JsonObject
                {
                    ["email"] = "2@email.com",
                    ["oid"] = "00000000-0000-0000-0000-000000000002"
                },
                new JsonObject
                {
                    ["email"] = "3@email.com",
                    ["oid"] = "00000000-0000-0000-0000-000000000003"
                },
            },
            ["azure_databricks_enterprise_oid"] =
                _resourceProvisionerConfiguration.Terraform.Variables.azure_databricks_enterprise_oid
        };
    }


}