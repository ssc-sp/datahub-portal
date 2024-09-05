using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class AzureStorageBlobTemplateTests
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
        var workspace =  GenerateTestTerraformWorkspace(workspaceAcronym);
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureStorageBlob, workspace);
        });
    }

    [Test]
    public async Task ShouldCopyAzureStorageBlobTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();

        // Setup new project template
        var newProjectTemplateExpectedFileCount = await SetupNewProjectTemplate(workspaceAcronym);
        var workspace =  GenerateTestTerraformWorkspace(workspaceAcronym);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureStorageBlob, workspace);

        var moduleSourcePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, TerraformTemplate.AzureStorageBlob);
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
    public async Task ShouldExtractAzureStorageBlobTemplateVariables()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        // Setup new project template
        await SetupNewProjectTemplate(workspaceAcronym);

        var workspace =  GenerateTestTerraformWorkspace(workspaceAcronym);
        var expectedVariables = GenerateExpectedVariables(workspace);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureStorageBlob, workspace);
        await _terraformService.ExtractVariables(TerraformTemplate.AzureStorageBlob, workspace);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
            $"{TerraformTemplate.AzureStorageBlob}.auto.tfvars.json");
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
    public async Task ShouldExtractAzureStorageBlobTemplateVariablesWithNoUsers()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        // Setup new project template
        await SetupNewProjectTemplate(workspaceAcronym);


        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var expectedVariables = GenerateExpectedVariables(workspace, false);

        await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureStorageBlob, workspace);
        await _terraformService.ExtractVariables(TerraformTemplate.AzureStorageBlob, workspace);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
            $"{TerraformTemplate.AzureStorageBlob}.auto.tfvars.json");
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
     public async Task ShouldExtractNewProjectTemplateVariablesWithoutDuplicates()
     {
         var workspaceAcronym = GenerateWorkspaceAcronym();
         // Setup new project template
         await SetupNewProjectTemplate(workspaceAcronym);

         var workspace =  GenerateTestTerraformWorkspace(workspaceAcronym);
         var expectedVariables = GenerateExpectedVariables(workspace);

         await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureStorageBlob, workspace);


         await _terraformService.ExtractVariables(TerraformTemplate.AzureStorageBlob, workspace);
         await _terraformService.ExtractVariables(TerraformTemplate.AzureStorageBlob, workspace);
         await _terraformService.ExtractVariables(TerraformTemplate.AzureStorageBlob, workspace);

         var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
             $"{TerraformTemplate.AzureStorageBlob}.auto.tfvars.json");
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