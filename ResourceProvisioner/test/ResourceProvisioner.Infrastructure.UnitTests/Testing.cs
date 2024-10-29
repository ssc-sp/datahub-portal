using Datahub.Shared;
using Datahub.Shared.Entities;
using Datahub.Shared.Enums;
using ResourceProvisioner.Application.Services;
using ResourceProvisioner.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ResourceProvisioner.Application.Config;
using ResourceProvisioner.Application.ResourceRun.Commands.CreateResourceRun;
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
    internal const string RequestingUserEmail = "unittest@user.com";
    internal const string RequestingAdminUser = "Unit Test Admin User";

    internal static readonly TerraformTemplate TestTemplate = new("TestModule", TerraformStatus.CreateRequested);

    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .AddUserSecrets<Testing>()
            .Build();

        _resourceProvisionerConfiguration = new ResourceProvisionerConfiguration();
        _configuration.Bind(_resourceProvisionerConfiguration);
        
        // Set the resource module branch to the latest dev branch
        _resourceProvisionerConfiguration.ModuleRepository.Branch = "dev";
        
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(Mock.Of<HttpClient>());
        
        
        var services = new ServiceCollection();
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<ITerraformService, TerraformService>();
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddSingleton(httpClientFactory.Object);
        services.AddSingleton(_configuration);
        services.AddSingleton(_resourceProvisionerConfiguration);
        var serviceProvider = services.BuildServiceProvider();
        
        _terraformService = serviceProvider.GetRequiredService<ITerraformService>();
        _repositoryService = serviceProvider.GetRequiredService<IRepositoryService>();

        // _terraformService = new TerraformService(Mock.Of<ILogger<TerraformService>>(),
            // _resourceProvisionerConfiguration, _configuration, _repositoryService);

        // _repositoryService = new RepositoryService(httpClientFactory.Object, Mock.Of<ILogger<RepositoryService>>(),
        //     _resourceProvisionerConfiguration, _terraformService);
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
        var versions = await _repositoryService.GetModuleVersions();
        var latestVersion = versions.Max();
        
        if(latestVersion == null)
            throw new Exception("No versions found for module repository");
        
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym,
            Version = $"v{latestVersion.ToString()}",
        };
        try
        {
            await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        }
        catch (IOException)
        {
            await Task.Delay(1000);
            await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(workspaceAcronym);
        }

        var module = new TerraformTemplate(TerraformTemplate.NewProjectTemplate,
            TerraformStatus.CreateRequested);

        await _terraformService.CopyTemplateAsync(module.Name, workspace);
        await _terraformService.ExtractVariables(module.Name, workspace);
        await _terraformService.ExtractBackendConfig(workspaceAcronym);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
        return Directory
            .GetFiles(moduleDestinationPath, "*.*", SearchOption.TopDirectoryOnly).Length;
    }

    internal static string GenerateWorkspaceAcronym()
    {
        return $"{Guid.NewGuid().ToString().Replace("-", "")[..8]}";
    }
    
    internal static CreateResourceRunCommand GenerateTestCreateResourceRunCommand(string workspaceAcronym, List<string> terraformTemplates, bool withUsers = true, string version = "latest")
    {
        return new CreateResourceRunCommand
        {
            Templates = terraformTemplates
                .Select(s => new TerraformTemplate(s, TerraformStatus.CreateRequested))
                .ToList(),
            Workspace = GenerateTestTerraformWorkspace(workspaceAcronym, withUsers, version),
            RequestingUserEmail = RequestingUser,
        };
    }

    internal static TerraformWorkspace GenerateTestTerraformWorkspace(string workspaceAcronym, bool withUsers = true, string version = "latest")
    {
        if (!withUsers)
        {
            return new TerraformWorkspace
            {
                Acronym = workspaceAcronym,
                Version = version
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
            Users = users,
            Version = version
        };
    }

    internal static TerraformTemplate GenerateTerraformTemplate(string template)
    {
        return new TerraformTemplate(template, TerraformStatus.CreateRequested);
    }
}