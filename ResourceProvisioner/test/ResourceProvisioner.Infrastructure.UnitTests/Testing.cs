using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Infrastructure.Common;

// ReSharper disable InconsistentNaming

namespace ResourceProvisioner.Infrastructure.UnitTests;

[SetUpFixture]
public class Testing
{
    internal static IConfiguration _configuration = null!;
    internal static IRepositoryService _repositoryService = null!;
    internal static ITerraformService _terraformService = null!;

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
    };

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        _resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        _configuration.Bind(_resourceProvisionerConfiguration);

        _terraformService = new TerraformService(Mock.Of<ILogger<TerraformService>>(),
            _resourceProvisionerConfiguration, _configuration);

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());
        _repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(),
            _resourceProvisionerConfiguration, _terraformService);
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
        try
        {
            await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        }
        catch (IOException ex)
        {
            await Task.Delay(1000);
            await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        }

        var module = new TerraformTemplate
        {
            Name = TerraformTemplate.NewProjectTemplate,
        };

        await _terraformService.CopyTemplateAsync(module, workspace);
        await _terraformService.ExtractVariables(module, workspace);
        await _terraformService.ExtractBackendConfig(workspaceAcronym);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
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

        var users = new List<TerraformUser>();
        const int numberOfOwners = 2;
        const int numberOfAdmins = 3;
        const int numberOfUsers = 10;
        const int numberOfGuests = 5;

        users.AddRange(Enumerable.Range(0, numberOfOwners)
            .Select(i => new TerraformUser
            {
                Email = $"owner{i}@email.com",
                ObjectId = Guid.NewGuid().ToString(),
                Role = Role.Owner
            }));
        
        users.AddRange(Enumerable.Range(0, numberOfAdmins)
            .Select(i => new TerraformUser
            {
                Email = $"admin{i}@email.com",
                ObjectId = Guid.NewGuid().ToString(),
                Role = Role.Admin
            }));
        
        users.AddRange(Enumerable.Range(0, numberOfUsers)
            .Select(i => new TerraformUser
            {
                Email = $"user{i}@email.com",
                ObjectId = Guid.NewGuid().ToString(),
                Role = Role.User
            }));
        
        users.AddRange(Enumerable.Range(0, numberOfGuests)
            .Select(i => new TerraformUser
            {
                Email = $"guest{i}@email.com",
                ObjectId = Guid.NewGuid().ToString(),
                Role = Role.Guest
            }));

        return new TerraformWorkspace
        {
            Acronym = workspaceAcronym,
            Users = users
        };
    }

    internal static TerraformTemplate GenerateTerraformTemplate(string template)
    {
        return new TerraformTemplate
        {
            Name = template,
        };
    }
}