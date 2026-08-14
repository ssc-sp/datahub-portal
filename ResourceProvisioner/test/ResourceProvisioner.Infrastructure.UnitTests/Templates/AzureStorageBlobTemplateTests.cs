using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;
using ResourceProvisioner.Infrastructure.UnitTests.Collections;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

[NonParallelizable]
[Category("TemplateTests")]
public class AzureStorageBlobTemplateTests : TemplateTestCollection
{
    [SetUp]
    public void RunBeforeEachTest()
    {
        var localModuleClonePath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
        var localInfrastructureClonePath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);

        VerifyDirectoryDoesNotExist(localModuleClonePath);
        VerifyDirectoryDoesNotExist(localInfrastructureClonePath);
    }

    [Test]
    public async Task ShouldThrowExceptionIfProjectNotInitialized()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();

        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, 
            new List<string> { TerraformTemplate.NewProjectTemplate });

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(command.Workspace);

        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureAppService, command);
        });
    }

    [Test]
    public async Task ShouldCopyAzureAppServiceTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var newProjectTemplateExpectedFileCount = await SetupNewProjectTemplate(workspaceAcronym);
        
        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, 
            new List<string> { TerraformTemplate.NewProjectTemplate });

        await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureAppService, command);

        var moduleSourcePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, TerraformTemplate.AzureAppService);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

        var expectedFiles = Directory.GetFiles(moduleSourcePath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(filename => !TerraformService.EXCLUDED_FILE_EXTENSIONS.Contains(Path.GetExtension(filename)))
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
            Assert.That(Directory.GetFiles(moduleDestinationPath), Has.Length.EqualTo(expectedFiles.Count + newProjectTemplateExpectedFileCount));
        });

        foreach (var file in expectedFiles)
        {
            var sourceFileContent = await File.ReadAllTextAsync(file);
            var expectedContent = sourceFileContent.Replace(TerraformService.TerraformTagToken, $"?ref={_resourceProvisionerConfiguration.ModuleRepository.Branch}-{command.Workspace.Version}");

            var destinationFileContent = await File.ReadAllTextAsync(Path.Join(moduleDestinationPath, Path.GetFileName(file)));
            Assert.That(destinationFileContent, Is.EqualTo(expectedContent));
        }
    }

    [Test]
    public async Task ShouldExtractNewProjectTemplateVariables()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);

        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, 
            new List<string> { TerraformTemplate.NewProjectTemplate });

        var expectedVariables = GenerateExpectedVariables(command.Workspace);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.NewProjectTemplate, command);
        await _terraformService.ExtractVariables(TerraformTemplate.NewProjectTemplate, command);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym), $"{TerraformTemplate.NewProjectTemplate}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables = JsonSerializer.Deserialize<JsonObject>(await File.ReadAllTextAsync(expectedVariablesFilename));

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
    public async Task ShouldExtractNewProjectTemplateVariablesWithNoUsers()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);

        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, 
            new List<string> { TerraformTemplate.NewProjectTemplate },
            false);

        var expectedVariables = GenerateExpectedVariables(command.Workspace, false);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.NewProjectTemplate, command);
        await _terraformService.ExtractVariables(TerraformTemplate.NewProjectTemplate, command);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym), $"{TerraformTemplate.NewProjectTemplate}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables = JsonSerializer.Deserialize<JsonObject>(await File.ReadAllTextAsync(expectedVariablesFilename));

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
    public async Task ShouldExtractNewProjectTemplateVariablesWithoutDuplicates()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);

        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, 
            new List<string> { TerraformTemplate.NewProjectTemplate });

        var expectedVariables = GenerateExpectedVariables(command.Workspace);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.NewProjectTemplate, command);
        await _terraformService.ExtractVariables(TerraformTemplate.NewProjectTemplate, command);
        await _terraformService.ExtractVariables(TerraformTemplate.NewProjectTemplate, command);
        await _terraformService.ExtractVariables(TerraformTemplate.NewProjectTemplate, command);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym), $"{TerraformTemplate.NewProjectTemplate}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables = JsonSerializer.Deserialize<JsonObject>(await File.ReadAllTextAsync(expectedVariablesFilename));

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
                [TerraformVariables.StorageContributorUsers] = new JsonArray(),
                [TerraformVariables.StorageGuestUsers] = new JsonArray(),
            };
        }

        return new JsonObject
        {
            [TerraformVariables.StorageContributorUsers] = new JsonArray(
                (workspace.Users ?? Array.Empty<TerraformUser>())
                .Where(u => u.Role is Role.Owner or Role.Admin or Role.User)
                .Select(u => new JsonObject
                {
                    ["email"] = u.Email,
                    ["oid"] = u.ObjectId,
                })
                .ToArray<JsonNode>()
            ),
            [TerraformVariables.StorageGuestUsers] = new JsonArray(
                (workspace.Users ?? Array.Empty<TerraformUser>())
                .Where(u => u.Role == Role.Guest)
                .Select(u => new JsonObject
                {
                    ["email"] = u.Email,
                    ["oid"] = u.ObjectId,
                })
                .ToArray<JsonNode>()
            )
        };
    }
}
