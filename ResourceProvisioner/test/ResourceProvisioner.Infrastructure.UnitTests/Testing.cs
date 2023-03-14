using Datahub.Shared.Entities;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Infrastructure.Common;

namespace ResourceProvisioner.Infrastructure.UnitTests;

[SetUpFixture]
public partial class Testing
{
    internal static IConfiguration _configuration = null!;
    internal static IRepositoryService _repositoryService;
    internal static ITerraformService _terraformService;
    
    internal static ResourceProvisionerConfiguration _resourceProvisionerConfiguration = null!;
    
    
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
        
        _resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        _configuration.Bind(_resourceProvisionerConfiguration);
        
        _terraformService = new TerraformService(Mock.Of<ILogger<TerraformService>>(), _configuration);
        
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());
        _repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(), _resourceProvisionerConfiguration, _terraformService);
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
    
    internal static async Task<int> SetupNewProjectTemplate(string workspaceAcronym)
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
    
    internal static string GenerateWorkspaceAcronym()
    {
        return $"{Guid.NewGuid().ToString().Replace("-", "")[..8]}";
    }
    
    internal static TerraformWorkspace GenerateTestTerraformWorkspace(string workspaceAcronym, bool withUsers = true)
    {
        if (!withUsers)
        {
            return new TerraformWorkspace
            {
                Acronym = workspaceAcronym,
            };
        }

        return new TerraformWorkspace
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
    }
    
    internal static TerraformTemplate GenerateTerraformTemplate(string template)
    {
        return new TerraformTemplate
        {
            Name = template,
            Version = "latest"
        };
    }
}