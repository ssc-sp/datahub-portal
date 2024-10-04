using Datahub.Shared.Entities;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.UnitTests;

using static Testing;

public class VariableUpdateTests
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
    public async Task ShouldNotExtractVariables()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var terraformWorkspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        await SetupNewProjectTemplate(workspaceAcronym);
        var template = GenerateTerraformTemplate(TerraformTemplate.VariableUpdate);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

        // get all the files and their last modified dates
        var files = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories);
        var fileDates = files.ToDictionary(file => file, File.GetLastWriteTime);

        await _terraformService.ExtractVariables(template.Name, terraformWorkspace);

        // assert that no new files were created
        Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
        var newFileCount = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories).Length;
        Assert.That(newFileCount, Is.EqualTo(files.Length));

        // assert that the files have not been modified
        foreach (var file in files)
        {
            Assert.That(File.GetLastWriteTime(file), Is.EqualTo(fileDates[file]));
        }
    }

    [Test]
    public async Task ShouldNotCopyTemplate()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var terraformWorkspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        await SetupNewProjectTemplate(workspaceAcronym);
        var template = GenerateTerraformTemplate(TerraformTemplate.VariableUpdate);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

        // get all the files and their last modified dates
        var files = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories);
        var fileDates = files.ToDictionary(file => file, File.GetLastWriteTime);

        await _terraformService.CopyTemplateAsync(template.Name, terraformWorkspace);

        // assert that no new files were created
        Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
        var newFileCount = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories).Length;
        Assert.That(newFileCount, Is.EqualTo(files.Length));

        // assert that the files have not been modified
        foreach (var file in files)
        {
            Assert.That(File.GetLastWriteTime(file), Is.EqualTo(fileDates[file]));
        }
    }

    [Test]
    public async Task ShouldExtractAllVariables()
    {
        var workspaceAcronym = GenerateWorkspaceAcronym();
        var terraformWorkspace = GenerateTestTerraformWorkspace(workspaceAcronym, false);
        await SetupNewProjectTemplate(workspaceAcronym);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);

        // get all the files and their last modified dates
        var files = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories);
        var fileDates = files.ToDictionary(file => file, File.GetLastWriteTime);

        await _terraformService.ExtractAllVariables(terraformWorkspace);

        // assert that no new files were created
        Assert.That(Directory.Exists(moduleDestinationPath), Is.True);
        var newFileCount = Directory.GetFiles(moduleDestinationPath, "*", SearchOption.AllDirectories).Length;
        Assert.That(newFileCount, Is.EqualTo(files.Length));

        // assert that the only files that have been modified are *.auto.tfvars.json
        foreach (var file in files)
        {
            if(file.EndsWith(".auto.tfvars.json"))
                Assert.That(File.GetLastWriteTime(file), Is.Not.EqualTo(fileDates[file]));
            else
                Assert.That(File.GetLastWriteTime(file), Is.EqualTo(fileDates[file]));
        }
    }
}