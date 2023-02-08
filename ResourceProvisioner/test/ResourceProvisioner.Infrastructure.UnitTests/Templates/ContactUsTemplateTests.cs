using System.Text.Json;
using System.Text.Json.Nodes;
using Datahub.Shared.Entities;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Templates;

using static Testing;

public class ContactUsTemplateTests
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
    public async Task ShouldNotCopyContactUsTemplate()
    {
        const string workspaceAcronym = "ShouldNotCopyContactUsTemplate";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym
        };
        await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);

        var module = new TerraformTemplate()
        {
            Name = TerraformTemplate.ContactUs,
            Version = "latest"
        };

        await _terraformService.CopyTemplateAsync(module, workspace);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);

        // verify that the directory does not exist
        Assert.That(Directory.Exists(moduleDestinationPath), Is.False);
    }

    [Test]
    public async Task ShouldNotCopyContactUsTemplateInExistingProject()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var workspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        var fileCount = await SetupNewProjectTemplate(workspaceAcronym);

        var module = GenerateTerraformTemplate(TerraformTemplate.ContactUs);
        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_configuration, workspaceAcronym);
        
        await _terraformService.CopyTemplateAsync(module, workspace);
        
        // assert that no new files were created
        Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
        var newFileCount = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories).Length;
        Assert.That(newFileCount, Is.EqualTo(fileCount));
    }
}