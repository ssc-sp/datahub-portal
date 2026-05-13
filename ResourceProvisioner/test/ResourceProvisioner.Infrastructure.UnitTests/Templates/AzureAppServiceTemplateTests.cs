using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;
using ResourceProvisioner.Infrastructure.UnitTests.Collections;
using TerraformVariables = Datahub.Shared.TerraformVariables;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

[NonParallelizable]
[Category("TemplateTests")]
public class AzureAppServiceTemplateTests : TemplateTestCollection
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

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(TestingWorkspace);

        var command = GenerateTestWorkspaceDefinition(
         workspaceAcronym, new List<string>()
         {
                        TerraformTemplate.NewProjectTemplate,
                        TerraformTemplate.NewProjectTemplate,
                        TerraformTemplate.NewProjectTemplate
         });

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
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureAppService);
        var command = GenerateTestWorkspaceDefinition(
              workspaceAcronym, new List<string>()
              {
                        TerraformTemplate.NewProjectTemplate,
                        TerraformTemplate.NewProjectTemplate,
                        TerraformTemplate.NewProjectTemplate
              });
        await _terraformService.CopyTemplateAsync(module.Name, command);

        await _repositoryService.FetchModuleRepository(command.Workspace.Version);

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
            var expectedContent = sourceFileContent.Replace(TerraformService.TerraformTagToken, $"?ref={_resourceProvisionerConfiguration.ModuleRepository.Branch}-{command.Workspace.Version}");
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

        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, new List<string>()
            {
                TerraformTemplate.NewProjectTemplate,
                TerraformTemplate.NewProjectTemplate,
                TerraformTemplate.NewProjectTemplate
            });
        
        
        await _terraformService.CopyTemplateAsync(module.Name, command);
        await _terraformService.ExtractVariables(module.Name, command);

        await _repositoryService.FetchModuleRepository(command.Workspace.Version);

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

        var command = GenerateTestWorkspaceDefinition(
         workspaceAcronym, new List<string>()
         {
                TerraformTemplate.NewProjectTemplate,
                TerraformTemplate.NewProjectTemplate,
                TerraformTemplate.NewProjectTemplate
         });
        await _terraformService.CopyTemplateAsync(module.Name, command);

        await _terraformService.ExtractVariables(module.Name, command);
        await _terraformService.ExtractVariables(module.Name, command);
        await _terraformService.ExtractVariables(module.Name, command);
        await _repositoryService.FetchModuleRepository(command.Workspace.Version);

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
            [TerraformVariables.AppServiceNameSuffix] = string.Empty
        };
    }
}