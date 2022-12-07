using System.Text.Json;
using System.Text.Json.Nodes;
using ResourceProvisioner.Domain.Entities;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class NewProjectTemplateTests
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
    public async Task ShouldCopyNewProjectTemplate()
    {
        const string workspaceAcronym = "new-template-test";
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        var module = new DataHubTemplate()
        {
            Name = TerraformService.NewProjectTemplate,
            Version = "latest"
        };

        await _terraformService.CopyTemplateAsync(module, workspaceAcronym);

        var moduleSourcePath = DirectoryUtils.GetTemplatePath(_configuration, TerraformService.NewProjectTemplate);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);

        // verify all the files are copied
        Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
        Assert.That(Directory.GetFiles(moduleSourcePath).Length,
            Is.EqualTo(Directory.GetFiles(moduleDestinationPath).Length));

        // go through each file and assert that the content is the same
        foreach (var file in Directory.GetFiles(moduleSourcePath))
        {
            var sourceFileContent = await File.ReadAllTextAsync(file);
            var destinationFileContent =
                await File.ReadAllTextAsync(Path.Join(moduleDestinationPath, Path.GetFileName(file)));
            Assert.That(sourceFileContent, Is.EqualTo(destinationFileContent));
        }
    }

    [Test]
    public async Task ShouldExtractNewProjectTemplateVariables()
    {
        const string workspaceAcronym = "ShouldExtractNewProjectTemplateVariables";

        var expectedVariables = new JsonObject
        {
            ["az_subscription_id"] = _configuration["Terraform:Variables:az_subscription_id"],
            ["az_tenant_id"] = _configuration["Terraform:Variables:az_tenant_id"],
            ["environment_classification"] = _configuration["Terraform:Variables:environment_classification"],
            ["environment_name"] = _configuration["Terraform:Variables:environment_name"],
            ["az_location"] = _configuration["Terraform:Variables:az_location"],
            ["resource_prefix"] = _configuration["Terraform:Variables:resource_prefix"],
            ["project_cd"] = "ShouldExtractNewProjectTemplateVariables",
            ["common_tags"] = new JsonObject
            {
                ["ClientOrganization"] = _configuration["Terraform:Variables:common_tags:ClientOrganization"],
                ["Environment"] = _configuration["Terraform:Variables:common_tags:Environment"],
                ["Sector"] = _configuration["Terraform:Variables:common_tags:Sector"],
            },
        };

        var module = new DataHubTemplate()
        {
            Name = TerraformService.NewProjectTemplate,
            Version = "latest"
        };

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _terraformService.CopyTemplateAsync(module, workspaceAcronym);


        await _terraformService.ExtractVariables(module, workspaceAcronym);

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
                Assert.That(expectedVariables[key]?.ToJsonString(), Is.EqualTo(value?.ToJsonString()));
            });
        }
    }

    [Test]
    public async Task ShouldExtractNewProjectTemplateVariablesWithoutDuplicates()
    {
        const string workspaceAcronym = "ShouldExtractNewProjectTemplateVariablesWithoutDuplicates";

        var expectedVariables = new JsonObject
        {
            ["az_subscription_id"] = _configuration["Terraform:Variables:az_subscription_id"],
            ["az_tenant_id"] = _configuration["Terraform:Variables:az_tenant_id"],
            ["environment_classification"] = _configuration["Terraform:Variables:environment_classification"],
            ["environment_name"] = _configuration["Terraform:Variables:environment_name"],
            ["az_location"] = _configuration["Terraform:Variables:az_location"],
            ["resource_prefix"] = _configuration["Terraform:Variables:resource_prefix"],
            ["project_cd"] = "ShouldExtractNewProjectTemplateVariablesWithoutDuplicates",
            ["common_tags"] = new JsonObject
            {
                ["ClientOrganization"] = _configuration["Terraform:Variables:common_tags:ClientOrganization"],
                ["Environment"] = _configuration["Terraform:Variables:common_tags:Environment"],
                ["Sector"] = _configuration["Terraform:Variables:common_tags:Sector"],
            },
        };

        var module = new DataHubTemplate()
        {
            Name = TerraformService.NewProjectTemplate,
            Version = "latest"
        };

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _terraformService.CopyTemplateAsync(module, workspaceAcronym);


        await _terraformService.ExtractVariables(module, workspaceAcronym);
        await _terraformService.ExtractVariables(module, workspaceAcronym);
        await _terraformService.ExtractVariables(module, workspaceAcronym);

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
                Assert.That(expectedVariables[key]?.ToJsonString(), Is.EqualTo(value?.ToJsonString()));
            });
        }
    }

    [Test]
    public async Task ShouldExtractBackendConfiguration()
    {
        const string workspaceAcronym = "ShouldExtractBackendConfiguration";

        var expectedConfiguration = @"resource_group_name = ""fsdh-core-test-rg""
storage_account_name = ""fsdhtestterraformbackend""
container_name = ""fsdh-project-states""
key = ""fsdh-ShouldExtractBackendConfiguration.tfstate""
";

        var module = new DataHubTemplate()
        {
            Name = TerraformService.NewProjectTemplate,
            Version = "latest"
        };
        
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _terraformService.CopyTemplateAsync(module, workspaceAcronym);
        await _terraformService.ExtractBackendConfig(workspaceAcronym);
        
        var expectedConfigurationFilename = Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym),
            "project.tfbackend");
        
        Assert.That(File.Exists(expectedConfigurationFilename), Is.True);
        Assert.That(await File.ReadAllTextAsync(expectedConfigurationFilename), Is.EqualTo(expectedConfiguration));
    }
}