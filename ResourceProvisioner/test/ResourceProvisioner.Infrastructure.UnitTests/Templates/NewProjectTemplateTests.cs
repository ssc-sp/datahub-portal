using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class NewProjectTemplateTests
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
    public async Task ShouldCopyNewProjectTemplate()
    {
        const string workspaceAcronym = "new-template-test";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        var module = new TerraformTemplate()
        {
            Name = TerraformTemplate.NewProjectTemplate,
        };

        await _terraformService.CopyTemplateAsync(module, workspace);

        var moduleSourcePath = DirectoryUtils.GetTemplatePath(_resourceProvisionerConfiguration, TerraformTemplate.NewProjectTemplate);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

        // verify all the files are copied except for the datahub readme
        var expectedFiles = Directory.GetFiles(moduleSourcePath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(filename => !TerraformService.EXCLUDED_FILE_EXTENSIONS.Contains(Path.GetExtension(filename)))
            .ToList();
        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
            Assert.That(Directory.GetFiles(moduleDestinationPath),
                Has.Length.EqualTo(expectedFiles.Count));
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
    public async Task ShouldExtractNewProjectTemplateVariables()
    {
        const string workspaceAcronym = "ShouldExtractNewProjectTemplateVariables";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };

        var expectedVariables = new JsonObject
        {
            ["az_subscription_id"] = _configuration["Terraform:Variables:az_subscription_id"],
            ["az_tenant_id"] = _configuration["Terraform:Variables:az_tenant_id"],
            ["datahub_app_sp_oid"] = _configuration["Terraform:Variables:datahub_app_sp_oid"],
            ["environment_classification"] = _configuration["Terraform:Variables:environment_classification"],
            ["environment_name"] = _configuration["Terraform:Variables:environment_name"],
            ["az_location"] = _configuration["Terraform:Variables:az_location"],
            ["resource_prefix"] = _configuration["Terraform:Variables:resource_prefix"],
            ["resource_prefix_alphanumeric"] = _configuration["Terraform:Variables:resource_prefix_alphanumeric"],
            ["storage_suffix"] = _configuration["Terraform:Variables:storage_suffix"],
            ["project_cd"] = "ShouldExtractNewProjectTemplateVariables",
            ["budget_amount"] = _resourceProvisionerConfiguration.Terraform.Variables.budget_amount,
            ["storage_size_limit_tb"] = _resourceProvisionerConfiguration.Terraform.Variables.storage_size_limit_tb,
            ["aad_admin_group_oid"] = _resourceProvisionerConfiguration.Terraform.Variables.aad_admin_group_oid,
            ["common_tags"] = new JsonObject
            {
                ["ClientOrganization"] = _configuration["Terraform:Variables:common_tags:ClientOrganization"],
                ["Environment"] = _configuration["Terraform:Variables:common_tags:Environment"],
                ["Sector"] = _configuration["Terraform:Variables:common_tags:Sector"],
            },
        };

        var module = new TerraformTemplate()
        {
            Name = TerraformTemplate.NewProjectTemplate,
        };

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _terraformService.CopyTemplateAsync(module, workspace);


        await _terraformService.ExtractVariables(module, workspace);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
            $"{module.Name}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables =
            JsonSerializer.Deserialize<JsonObject>(
                await File.ReadAllTextAsync(expectedVariablesFilename));

        foreach (var (key, value) in actualVariables!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(expectedVariables.ContainsKey(key), Is.True, $"Missing variable {key}");
                Assert.That(expectedVariables[key]?.ToJsonString(), Is.EqualTo(value?.ToJsonString()));
            });
        }
    }

    [Test]
    public async Task ShouldExtractNewProjectTemplateVariablesWithoutDuplicates()
    {
        const string workspaceAcronym = "ShouldExtractNewProjectTemplateVariablesWithoutDuplicates";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };

        var expectedVariables = new JsonObject
        {
            ["az_subscription_id"] = _resourceProvisionerConfiguration.Terraform.Variables.az_subscription_id,
            ["az_tenant_id"] = _resourceProvisionerConfiguration.Terraform.Variables.az_tenant_id,
            ["datahub_app_sp_oid"] = _resourceProvisionerConfiguration.Terraform.Variables.datahub_app_sp_oid,
            ["environment_classification"] = _resourceProvisionerConfiguration.Terraform.Variables.environment_classification,
            ["environment_name"] = _resourceProvisionerConfiguration.Terraform.Variables.environment_name,
            ["az_location"] = _resourceProvisionerConfiguration.Terraform.Variables.az_location,
            ["resource_prefix"] = _resourceProvisionerConfiguration.Terraform.Variables.resource_prefix,
            ["resource_prefix_alphanumeric"] = _resourceProvisionerConfiguration.Terraform.Variables.resource_prefix_alphanumeric,
            ["resource_suffix"] = _resourceProvisionerConfiguration.Terraform.Variables.storage_suffix,
            ["budget_amount"] = _resourceProvisionerConfiguration.Terraform.Variables.budget_amount,
            ["storage_size_limit_tb"] = _resourceProvisionerConfiguration.Terraform.Variables.storage_size_limit_tb,
            ["aad_admin_group_oid"] = _resourceProvisionerConfiguration.Terraform.Variables.aad_admin_group_oid,
            ["project_cd"] = "ShouldExtractNewProjectTemplateVariablesWithoutDuplicates",
            ["common_tags"] = new JsonObject
            {
                ["ClientOrganization"] = _resourceProvisionerConfiguration.Terraform.Variables.common_tags.ClientOrganization,
                ["Environment"] = _resourceProvisionerConfiguration.Terraform.Variables.common_tags.Environment,
                ["Sector"] = _resourceProvisionerConfiguration.Terraform.Variables.common_tags.Sector,
            },
        };

        var module = new TerraformTemplate()
        {
            Name = TerraformTemplate.NewProjectTemplate,
        };

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _terraformService.CopyTemplateAsync(module, workspace);


        await _terraformService.ExtractVariables(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);

        var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
            $"{module.Name}.auto.tfvars.json");
        Assert.That(File.Exists(expectedVariablesFilename), Is.True);

        var actualVariables =
            JsonSerializer.Deserialize<JsonObject>(
                await File.ReadAllTextAsync(expectedVariablesFilename));

        foreach (var (key, value) in actualVariables!)
        {
            Assert.Multiple(() =>
            {
                Assert.That(expectedVariables.ContainsKey(key), Is.True, $"Missing variable {key}");
                Assert.That(expectedVariables[key]?.ToJsonString(), Is.EqualTo(value?.ToJsonString()));
            });
        }
    }

    [Test]
    public async Task ShouldExtractBackendConfiguration()
    {
        const string workspaceAcronym = "ShouldExtractBackendConfiguration";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };

        var expectedConfiguration = @"resource_group_name = ""fsdh-core-test-rg""
storage_account_name = ""fsdhtestterraformbackend""
container_name = ""fsdh-project-states""
key = ""fsdh-ShouldExtractBackendConfiguration.tfstate""
";

        var module = new TerraformTemplate()
        {
            Name = TerraformTemplate.NewProjectTemplate,
        };

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _terraformService.CopyTemplateAsync(module, workspace);
        await _terraformService.ExtractBackendConfig(workspaceAcronym);

        var expectedConfigurationFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
            "project.tfbackend");

        Assert.That(File.Exists(expectedConfigurationFilename), Is.True);
        Assert.That(await File.ReadAllTextAsync(expectedConfigurationFilename), Is.EqualTo(expectedConfiguration));
    }
    
    [Test]
    public async Task ShouldSkipExtractBackendConfigurationIfExists()
    {
        const string workspaceAcronym = "ShouldSkipExtractBackendConfigurationIfExists";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym,
            Version = "latest"
        };

        var module = new TerraformTemplate()
        {
            Name = TerraformTemplate.NewProjectTemplate,
        };

        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _terraformService.CopyTemplateAsync(module, workspace);
        
        // Write a fake backend config before extracting
        var expectedConfigurationFilename = Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym),
            "project.tfbackend");
        var existingConfiguration = "test";
        Directory.CreateDirectory(Path.Join(DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym)));
        await File.WriteAllTextAsync(expectedConfigurationFilename, existingConfiguration);
        
        
        await _terraformService.ExtractBackendConfig(workspaceAcronym);

        Assert.That(File.Exists(expectedConfigurationFilename), Is.True);
        Assert.That(await File.ReadAllTextAsync(expectedConfigurationFilename), Is.EqualTo(existingConfiguration));
    }

    [Test]
    [TestCase("v2.7.0")]
    [TestCase("v2.8.0")]
    public async Task ShouldCheckoutVersionCorrectly(string version)
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var command = GenerateTestCreateResourceRunCommand(
            workspaceAcronym, new List<string>() { TerraformTemplate.NewProjectTemplate }, true, version);
        
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        await _repositoryService.ExecuteResourceRuns(command.Templates, command.Workspace, command.RequestingUserEmail);
    
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
        
        // verify that the file main.tf does not contain "{{version}}" or "{{branch}}"
        var mainTfPath = Path.Join(moduleDestinationPath, "main.tf");
        var mainTfContent = await File.ReadAllTextAsync(mainTfPath);
        Assert.That(mainTfContent, Does.Not.Contain(TerraformService.TerraformVersionToken));
        Assert.That(mainTfContent, Does.Not.Contain(TerraformService.TerraformBranchToken));
    }
}