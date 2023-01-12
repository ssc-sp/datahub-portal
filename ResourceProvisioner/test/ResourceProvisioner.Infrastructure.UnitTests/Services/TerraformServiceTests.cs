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
        var localModuleClonePath = DirectoryUtils.GetModuleRepositoryPath(_configuration);
        var localInfrastructureClonePath = DirectoryUtils.GetInfrastructureRepositoryPath(_configuration);
        
        VerifyDirectoryDoesNotExist(localModuleClonePath);
        VerifyDirectoryDoesNotExist(localInfrastructureClonePath);
    }
    
    [Test]
    public void ShouldThrowExceptionWhenProjectNotInitialized()
    {
        var moduleDestinationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _configuration["InfrastructureRepository:LocalPath"]);
        var module = new TerraformTemplate()
        {
            Name = "azure-storage-blob",
            Version = "latest"
        };
        
        Assert.That(Directory.Exists(moduleDestinationPath), Is.False);
        Assert.ThrowsAsync<ProjectNotInitializedException>(async () =>
        {
            await _terraformService.CopyTemplateAsync(module, TestingWorkspace);
        });
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
    }
  }
}
";
        var variables = TerraformService.ParseVariableDefinitions(variableJson);
        
        Assert.That(variables["datahub_rg_name"], Is.EqualTo("string"));
        Assert.That(variables["location"], Is.EqualTo("string"));
    }

}