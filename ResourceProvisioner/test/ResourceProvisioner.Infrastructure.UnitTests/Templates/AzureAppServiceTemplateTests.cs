using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;
using TerraformVariables = Datahub.Shared.TerraformVariables;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class AzureAppServiceTemplateTests
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

        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureAppService, workspace);
        });
    }

    [Test]
    public async Task ShouldCopyAzureAppServiceTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var newProjectTemplateExpectedFileCount = await SetupNewProjectTemplate(workspaceAcronym);
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureAppService);

        await _terraformService.CopyTemplateAsync(module.Name, workspace);

        await _repositoryService.FetchModuleRepository();

        var moduleSourcePath =
            DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, TerraformTemplate.AzureAppService);
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
    public async Task ShouldExtractAzureAppServiceTemplateVariables()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);

        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureAppService);
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
                Assert.That(value?.ToJsonString(), Is.EqualTo(expectedVariables[key]?.ToJsonString()), $"Expected variable {key} does not match actual value");
            });
        }
    }

    [Test]
    public async Task ShouldExtractAzureAppServiceTemplateVariablesWithoutDuplicates()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        await SetupNewProjectTemplate(workspaceAcronym);

        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym);
        var expectedVariables = GenerateExpectedVariables(workspace);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureAppService);

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

    private static JsonObject GenerateExpectedVariables(TerraformWorkspace workspace)
    {
        return new JsonObject
        {
            [TerraformVariables.AllowSourceIp] = _resourceProvisionerConfiguration.Terraform.Variables.allow_source_ip,
        };
    }
}