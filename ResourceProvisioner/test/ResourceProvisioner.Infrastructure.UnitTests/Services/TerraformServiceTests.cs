using Datahub.Shared.Entities;
using ResourceProvisioner.Domain.Exceptions;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Services;

namespace ResourceProvisioner.Infrastructure.UnitTests.Services;

using static Testing;
public class TerraformServiceTests
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
    public void ShouldThrowExceptionWhenProjectNotInitialized()
    {
        var moduleDestinationPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, _resourceProvisionerConfiguration.InfrastructureRepository.LocalPath, DirectoryUtils.tempDirectory, _resourceProvisionerConfiguration.ModuleRepository.Name);
        Assert.That(Directory.Exists(moduleDestinationPath), Is.False);
        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(TerraformTemplate.AzureStorageBlob, TestingWorkspace);
        });
    }

    [Test]
    public void ShouldNotDeleteAutoTfvarsJsonFiles()
    {
        string[] fileNames = { "app.tf", "app-template.variables.tf.json", "app.auto.tfvars.json" };
        var newFileList = fileNames
            .Where(file => !file.EndsWith(".auto.tfvars.json"))
            .ToArray();

        Assert.That(newFileList.Length, Is.EqualTo(2));
    }

    [Test]
    public void ShouldParseTerraformVariables()
    {
        var variableJson = @"{
  ""variable"": {
    ""datahub_rg_name"": {
      ""type"": ""string"",
      ""default"": ""sp-datahub-unit-test-rg""
    },
    ""location"": {
      ""type"": ""string"",
      ""default"": ""canadacentral""
    },
    ""no_default"": {
      ""type"": ""string""
    }
  }
}
";
        var variables = TerraformService.ParseVariableDefinitions(variableJson);
        
        Assert.That(variables["datahub_rg_name"].Item1, Is.EqualTo("string"));
        Assert.That(variables["datahub_rg_name"].Item2, Is.False);
        
        Assert.That(variables["location"].Item1, Is.EqualTo("string"));
        Assert.That(variables["location"].Item2, Is.False);
        
        Assert.That(variables["no_default"].Item1, Is.EqualTo("string"));
        Assert.That(variables["no_default"].Item2, Is.True);
    }

}