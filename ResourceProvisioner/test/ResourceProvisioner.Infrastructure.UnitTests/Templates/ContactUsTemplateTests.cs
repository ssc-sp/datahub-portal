using Datahub.Shared.Entities;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.UnitTests.Collections;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

[NonParallelizable]
[Category("TemplateTests")]
public class ContactUsTemplateTests : TemplateTestCollection
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
    public async Task ShouldNotCopyContactUsTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, new List<string>()
            {
                   TerraformTemplate.NewProjectTemplate,
                   TerraformTemplate.NewProjectTemplate,
                   TerraformTemplate.NewProjectTemplate
            });
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(TestingWorkspace);

        var module = GenerateTerraformTemplate(TerraformTemplate.ContactUs);

        await _terraformService.CopyTemplateAsync(module.Name, command);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

        // verify that the directory does not exist
        Assert.That(Directory.Exists(moduleDestinationPath), Is.False);
    }

    [Test]
    public async Task ShouldNotCopyContactUsTemplateInExistingProject()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var fileCount = await SetupNewProjectTemplate(workspaceAcronym);
        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, new List<string>()
            {
                   TerraformTemplate.NewProjectTemplate,
                   TerraformTemplate.NewProjectTemplate,
                   TerraformTemplate.NewProjectTemplate
            });
        var module = GenerateTerraformTemplate(TerraformTemplate.ContactUs);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
        
        await _terraformService.CopyTemplateAsync(module.Name, command);
        
        // assert that no new files were created
        Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
        var newFileCount = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories).Length;
        Assert.That(newFileCount, Is.EqualTo(fileCount));
    }
}