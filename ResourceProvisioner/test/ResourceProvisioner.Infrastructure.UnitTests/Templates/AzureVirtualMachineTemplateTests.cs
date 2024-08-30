using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class AzureVirtualMachineTemplateTests
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
    public async Task ShouldNotCopyAzureVirtualMachineTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        var module = GenerateTerraformTemplate(TerraformTemplate.AzureVirtualMachine);

        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(module.Name, workspace);
        });

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

        // verify that the directory does not exist
        Assert.That(Directory.Exists(moduleDestinationPath), Is.False);
    }

    [Test]
    public async Task ShouldNotCopyAzureVirtualMachineTemplateInExistingProject()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var fileCount = await SetupNewProjectTemplate(workspaceAcronym);

        var module = GenerateTerraformTemplate(TerraformTemplate.AzureVirtualMachine);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
        
        await _terraformService.CopyTemplateAsync(module.Name, workspace);
        
        // assert that no new files were created
        Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
        var newFileCount = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories).Length;
        Assert.That(newFileCount, Is.EqualTo(fileCount));
    }
}