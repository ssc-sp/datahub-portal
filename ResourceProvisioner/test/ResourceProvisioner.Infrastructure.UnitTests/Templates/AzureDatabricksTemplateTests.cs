using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;
using TerraformVariables = Datahub.Shared.TerraformVariables;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class AzureDatabricksTemplateTests
{
    [SetUp]
    public void RunBeforeEachTest()
    {
        var localModuleClonePath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
        var localInfrastructureClonePath =
            DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);

        VerifyDirectoryDoesNotExist(localModuleClonePath);
        VerifyDirectoryDoesNotExist(localInfrastructureClonePath);
    }

    [Test]
    public async Task ShouldThrowExceptionIfProjectNotInitialized()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);

        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(module.Name, workspace);
        });
    }

    [Test]
    public async Task ShouldCopyAzureDatabricksTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var newProjectTemplateExpectedFileCount = await SetupNewProjectTemplate(workspaceAcronym);
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);

        await _terraformService.CopyTemplateAsync(module.Name, workspace);

        await _repositoryService.FetchModuleRepository();

        var moduleSourcePath =
            DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, TerraformTemplate.AzureDatabricks);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

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
            var expectedContent = sourceFileContent
                .Replace(TerraformService.TerraformVersionToken, workspace.Version)
                .Replace(TerraformService.TerraformBranchToken, $"?ref={_resourceProvisionerConfiguration.ModuleRepository.Branch}");
            var destinationFileContent =
                await File.ReadAllTextAsync(Path.Join(moduleDestinationPath, Path.GetFileName(file)));
            Assert.That(destinationFileContent, Is.EqualTo(expectedContent));
        }
    }

    [Test]
    public async Task ShouldExtractAzureDatabricksTemplateVariables()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);

        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);
        var expectedVariables = GenerateExpectedVariables(workspace);

        await _terraformService.CopyTemplateAsync(module.Name, workspace);
        await _terraformService.ExtractVariables(module.Name, workspace);

        var expectedVariablesFilename = Path.Join(
            DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
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
        var expectedVariables = GenerateExpectedVariables(workspace, false);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);

        await _terraformService.CopyTemplateAsync(module.Name, workspace);
        await _terraformService.ExtractVariables(module.Name, workspace);

        var expectedVariablesFilename = Path.Join(
            DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
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
        var expectedVariables = GenerateExpectedVariables(workspace);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureDatabricks);

        await _terraformService.CopyTemplateAsync(module.Name, workspace);

        await _terraformService.ExtractVariables(module.Name, workspace);
        await _terraformService.ExtractVariables(module.Name, workspace);
        await _terraformService.ExtractVariables(module.Name, workspace);

        var expectedVariablesFilename = Path.Join(
            DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
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

    private static JsonObject GenerateExpectedVariables(TerraformWorkspace workspace, bool withUsers = true)
    {
        if (!withUsers)
        {
            return new JsonObject
            {
                [TerraformVariables.DatabricksProjectLeadUsers] = new JsonArray(),
                [TerraformVariables.DatabricksAdminUsers] = new JsonArray(),
                [TerraformVariables.DatabricksProjectUsers] = new JsonArray(),
                [TerraformVariables.DatabricksProjectGuests] = new JsonArray(),
                [TerraformVariables.AzureDatabricksEnterpriseOid] = _resourceProvisionerConfiguration.Terraform
                    .Variables
                    .azure_databricks_enterprise_oid,
                [TerraformVariables.AzureLogWorkspaceId] =
                    _resourceProvisionerConfiguration.Terraform.Variables.log_workspace_id,
            };
        }

        return new JsonObject
        {
            [TerraformVariables.DatabricksProjectLeadUsers] = new JsonArray(
                (workspace.Users ?? Array.Empty<TerraformUser>())
                .Where(u => u.Role == Role.Owner)
                .Select(u => new JsonObject
                {
                    ["email"] = u.Email,
                    ["oid"] = u.ObjectId,
                })
                .ToArray<JsonNode>()
            ),
            [TerraformVariables.DatabricksAdminUsers] = new JsonArray(
                (workspace.Users ?? Array.Empty<TerraformUser>())
                .Where(u => u.Role == Role.Admin)
                .Select(u => new JsonObject
                {
                    ["email"] = u.Email,
                    ["oid"] = u.ObjectId,
                })
                .ToArray<JsonNode>()
            ),
            [TerraformVariables.DatabricksProjectUsers] = new JsonArray(
                (workspace.Users ?? Array.Empty<TerraformUser>())
                .Where(u => u.Role == Role.User)
                .Select(u => new JsonObject
                {
                    ["email"] = u.Email,
                    ["oid"] = u.ObjectId,
                })
                .ToArray<JsonNode>()
            ),
            [TerraformVariables.DatabricksProjectGuests] = new JsonArray(
                (workspace.Users ?? Array.Empty<TerraformUser>())
                .Where(u => u.Role == Role.Guest)
                .Select(u => new JsonObject
                {
                    ["email"] = u.Email,
                    ["oid"] = u.ObjectId,
                })
                .ToArray<JsonNode>()
            ),
            [TerraformVariables.AzureDatabricksEnterpriseOid] =
                _resourceProvisionerConfiguration.Terraform.Variables.azure_databricks_enterprise_oid,
            [TerraformVariables.AzureLogWorkspaceId] =
                _resourceProvisionerConfiguration.Terraform.Variables.log_workspace_id,
        };
    }
}