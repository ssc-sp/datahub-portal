using Datahub.Shared.Entities;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace ResourceProvisioner.Infrastructure.UnitTests;

[SetUpFixture]
public partial class Testing
{
    internal static IConfiguration _configuration = null!;
    internal static IRepositoryService _repositoryService;
    internal static ITerraformService _terraformService;
    
    
    internal const string ProjectAcronym = "TEST";
    internal static TerraformWorkspace TestingWorkspace => new()
    {
        Acronym = ProjectAcronym,
    };
    internal const string RequestingUser = "Unit Test User";
    internal const string RequestingAdminUser = "Unit Test Admin User";
    internal static readonly TerraformTemplate TestTemplate = new()
    {
        Name = "TestModule",
        Version = "1.0.0",
    };
    
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();
        
        _terraformService = new TerraformService(Mock.Of<ILogger<TerraformService>>(), _configuration);
        
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());
        _repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(), _configuration, _terraformService);
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
    }
    
    internal static void VerifyDirectoryDoesNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }
        
        var dir = new DirectoryInfo(path);
        SetAttributesNormal(dir);
        try
        {
            dir.Delete(true);
            
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    internal static void SetAttributesNormal(DirectoryInfo dir)
    {
        foreach (var subDir in dir.GetDirectories())
            SetAttributesNormal(subDir);
        foreach (var file in dir.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
        }
    }
}