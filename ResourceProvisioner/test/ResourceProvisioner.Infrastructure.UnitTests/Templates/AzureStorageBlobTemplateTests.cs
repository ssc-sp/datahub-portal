using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
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
        var localModuleClonePath = DirectoryUtils.GetModuleRepositoryPath(_configuration);
        var localInfrastructureClonePath = DirectoryUtils.GetInfrastructureRepositoryPath(_configuration);

        VerifyDirectoryDoesNotExist(localModuleClonePath);
        VerifyDirectoryDoesNotExist(localInfrastructureClonePath);
    }

    [Test]
    public async Task ShouldThrowExceptionIfProjectNotInitialized()
    {
        const string workspaceAcronym = "ShouldThrowExceptionIfProjectNotInitialized";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        var module = new TerraformTemplate
        {
            Name = TerraformTemplate.AzureStorageBlob,
            Version = "latest"
        };

        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(module, workspace);
        });
    }

    [Test]
    public async Task ShouldCopyAzureStorageBlobTemplate()
    {
        const string workspaceAcronym = "ShouldCopyAzureStorageBlobTemplate";

        // Setup new project template
        var newProjectTemplateExpectedFileCount = await SetupNewProjectTemplate(workspaceAcronym);

        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };

        var module = new TerraformTemplate
        {
            Name = TerraformTemplate.AzureStorageBlob,
            Version = "latest"
        };

        await _terraformService.CopyTemplateAsync(module, workspace);

        var moduleSourcePath = DirectoryUtils.GetTemplatePath(_configuration, TerraformTemplate.AzureStorageBlob);
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
    public async Task ShouldExtractAzureStorageBlobTemplateVariables()
    {
        const string workspaceAcronym = "ShouldExtractAzureStorageBlobTemplateVariables";
        // Setup new project template
        await SetupNewProjectTemplate(workspaceAcronym);

        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym,
            Users = new List<TerraformUser>
            {
                new()
                {
                    Email = "1@email.com",
                    ObjectId = "00000000-0000-0000-0000-000000000001"
                },
                new()
                {
                    Email = "2@email.com",
                    ObjectId = "00000000-0000-0000-0000-000000000002"
                },
                new()
                {
                    Email = "3@email.com",
                    ObjectId = "00000000-0000-0000-0000-000000000003"
                }
            }
        };

        var expectedVariables = new JsonObject
        {
            ["storage_contributor_users"] = new JsonArray
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
        };

        var module = new TerraformTemplate
        {
            Name = TerraformTemplate.AzureStorageBlob,
            Version = "latest"
        };

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
    public async Task ShouldExtractAzureStorageBlobTemplateVariablesWithNoUsers()
    {
        const string workspaceAcronym = "ShouldExtractAzureStorageBlobTemplateVariablesWithNoUsers";
        // Setup new project template
        await SetupNewProjectTemplate(workspaceAcronym);


        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym,
        };

        var expectedVariables = new JsonObject
        {
            ["storage_contributor_users"] = new JsonArray()
        };

        var module = new TerraformTemplate
        {
            Name = TerraformTemplate.AzureStorageBlob,
            Version = "latest"
        };

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
//
//     [Test]
//     public async Task ShouldExtractNewProjectTemplateVariablesWithoutDuplicates()
//     {
//         const string workspaceAcronym = "ShouldExtractNewProjectTemplateVariablesWithoutDuplicates";
//         var workspace = new TerraformWorkspace
//         {
//             Acronym = workspaceAcronym
//         };
//
//         var expectedVariables = new JsonObject
//         {
//             ["az_subscription_id"] = _resourceProvisionerConfiguration.Terraform.Variables.az_subscription_id,
//             ["az_tenant_id"] = _resourceProvisionerConfiguration.Terraform.Variables.az_tenant_id,
//             ["datahub_app_sp_oid"] = _resourceProvisionerConfiguration.Terraform.Variables.datahub_app_sp_oid,
//             ["environment_classification"] = _resourceProvisionerConfiguration.Terraform.Variables.environment_classification,
//             ["environment_name"] = _resourceProvisionerConfiguration.Terraform.Variables.environment_name,
//             ["az_location"] = _resourceProvisionerConfiguration.Terraform.Variables.az_location,
//             ["resource_prefix"] = _resourceProvisionerConfiguration.Terraform.Variables.resource_prefix,
//             ["project_cd"] = "ShouldExtractNewProjectTemplateVariablesWithoutDuplicates",
//             ["common_tags"] = new JsonObject
//             {
//                 ["ClientOrganization"] = _resourceProvisionerConfiguration.Terraform.Variables.common_tags.ClientOrganization,
//                 ["Environment"] = _resourceProvisionerConfiguration.Terraform.Variables.common_tags.Environment,
//                 ["Sector"] = _resourceProvisionerConfiguration.Terraform.Variables.common_tags.Sector,
//             },
//         };
//
//         var module = new TerraformTemplate()
//         {
//             Name = TerraformTemplate.NewProjectTemplate,
//             Version = "latest"
//         };
//
//         await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
//         await _terraformService.CopyTemplateAsync(module, workspace);
//
//
//         await _terraformService.ExtractVariables(module, workspace);
//         await _terraformService.ExtractVariables(module, workspace);
//         await _terraformService.ExtractVariables(module, workspace);
//
//         var expectedVariablesFilename = Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym),
//             $"{module.Name}.auto.tfvars.json");
//         Assert.That(File.Exists(expectedVariablesFilename), Is.True);
//
//         var actualVariables =
//             JsonSerializer.Deserialize<JsonObject>(
//                 await File.ReadAllTextAsync(expectedVariablesFilename));
//
//         foreach (var (key, value) in actualVariables!)
//         {
//             Assert.Multiple(() =>
//             {
//                 Assert.That(expectedVariables.ContainsKey(key), Is.True);
//                 Assert.That(expectedVariables[key]?.ToJsonString(), Is.EqualTo(value?.ToJsonString()));
//             });
//         }
//     }
//
//     [Test]
//     public async Task ShouldExtractBackendConfiguration()
//     {
//         const string workspaceAcronym = "ShouldExtractBackendConfiguration";
//         var workspace = new TerraformWorkspace
//         {
//             Acronym = workspaceAcronym
//         };
//
//         var expectedConfiguration = @"resource_group_name = ""fsdh-core-test-rg""
// storage_account_name = ""fsdhtestterraformbackend""
// container_name = ""fsdh-project-states""
// key = ""fsdh-ShouldExtractBackendConfiguration.tfstate""
// ";
//
//         var module = new TerraformTemplate()
//         {
//             Name = TerraformTemplate.NewProjectTemplate,
//             Version = "latest"
//         };
//
//         await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
//         await _terraformService.CopyTemplateAsync(module, workspace);
//         await _terraformService.ExtractBackendConfig(workspaceAcronym);
//
//         var expectedConfigurationFilename = Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym),
//             "project.tfbackend");
//
//         Assert.That(File.Exists(expectedConfigurationFilename), Is.True);
//         Assert.That(await File.ReadAllTextAsync(expectedConfigurationFilename), Is.EqualTo(expectedConfiguration));
//     }
//     
//     [Test]
//     public async Task ShouldSkipExtractBackendConfigurationIfExists()
//     {
//         const string workspaceAcronym = "ShouldSkipExtractBackendConfigurationIfExists";
//         var workspace = new TerraformWorkspace
//         {
//             Acronym = workspaceAcronym
//         };
//
//         var module = new TerraformTemplate()
//         {
//             Name = TerraformTemplate.NewProjectTemplate,
//             Version = "latest"
//         };
//
//         await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
//         await _terraformService.CopyTemplateAsync(module, workspace);
//         
//         // Write a fake backend config before extracting
//         var expectedConfigurationFilename = Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym),
//             "project.tfbackend");
//         var existingConfiguration = "test";
//         Directory.CreateDirectory(Path.Join(DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym)));
//         await File.WriteAllTextAsync(expectedConfigurationFilename, existingConfiguration);
//         
//         
//         await _terraformService.ExtractBackendConfig(workspaceAcronym);
//
//         Assert.That(File.Exists(expectedConfigurationFilename), Is.True);
//         Assert.That(await File.ReadAllTextAsync(expectedConfigurationFilename), Is.EqualTo(existingConfiguration));
//     }


    private static async Task<int> SetupNewProjectTemplate(string workspaceAcronym)
    {
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        var module = new TerraformTemplate
        {
            Name = TerraformTemplate.NewProjectTemplate,
            Version = "latest"
        };

        await _terraformService.CopyTemplateAsync(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);
        await _terraformService.ExtractBackendConfig(workspaceAcronym);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);
        return Directory
            .GetFiles(moduleDestinationPath, "*.*", SearchOption.TopDirectoryOnly).Length;
    }
}