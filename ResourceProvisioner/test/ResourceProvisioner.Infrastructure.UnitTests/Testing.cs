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

// Assembly-level attribute to disable parallel execution
[assembly: NonParallelizable]

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
        Version = "v5.0.4",
    };

    internal const string RequestingUser = "Unit Test User";
    internal const string RequestingUserEmail = "unittest@user.com";
    internal const string RequestingAdminUser = "Unit Test Admin User";
    internal const string ResourceGroup = "TestResourceGroup";

    internal static readonly TerraformTemplate TestTemplate = new("TestModule", TerraformStatus.CreateRequested, DateTime.UtcNow);

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

        // Clean up any existing test directories before starting
        CleanupAllTestDirectories();
    }

    [OneTimeTearDown]
    public void RunAfterAnyTests()
    {
        // Final cleanup after all tests complete
        CleanupAllTestDirectories();
    }
    
    private static void CleanupAllTestDirectories()
    {
        try
        {
            var localModuleClonePath = DirectoryUtils.GetModuleRepositoryPath(_resourceProvisionerConfiguration);
            var localInfrastructureClonePath = DirectoryUtils.GetInfrastructureRepositoryPath(_resourceProvisionerConfiguration);
            
            VerifyDirectoryDoesNotExist(localModuleClonePath);
            VerifyDirectoryDoesNotExist(localInfrastructureClonePath);
        }
        catch (Exception)
        {
            // Ignore cleanup failures during teardown
        }
    }
    
    internal static void VerifyDirectoryDoesNotExist(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var dir = new DirectoryInfo(path);
        SetAttributesNormal(dir);
        
        // Add retry logic for file system operations with exponential backoff
        var maxRetries = 5;
        var baseDelay = TimeSpan.FromMilliseconds(200);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                dir.Delete(true);
                break;
            }
            catch (UnauthorizedAccessException) when (i < maxRetries - 1)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, i)));
                SetAttributesNormal(dir); // Try to reset attributes again
            }
            catch (DirectoryNotFoundException)
            {
                // Directory is already deleted, which is what we want
                break;
            }
            catch (IOException) when (i < maxRetries - 1)
            {
                // File might be in use (like Git pack files), wait with exponential backoff
                Thread.Sleep(TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, i)));
                
                // Force garbage collection to help release file handles
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        
        // If we still have issues after retries, try a forced approach
        if (Directory.Exists(path))
        {
            try
            {
                // Try to kill any Git processes that might be holding files
                var gitProcesses = System.Diagnostics.Process.GetProcessesByName("git");
                foreach (var process in gitProcesses)
                {
                    try { process.Kill(); } catch { /* ignore */ }
                }
                Thread.Sleep(1000);
                new DirectoryInfo(path).Delete(true);
            }
            catch 
            {
                // Final fallback - just ignore the error and proceed
                // The test might fail but won't hang
            }
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
        var latestVersion = "latest";
        var workspace = new TerraformWorkspace
        {
            Acronym = workspaceAcronym,
            Version = $"v{latestVersion.ToString()}",
        };
        try
        {
            await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(TestingWorkspace);
        }
        catch (IOException)
        {
            await Task.Delay(1000);
            await _repositoryService.FetchRepositoriesAndCheckoutProjectBranch(TestingWorkspace);
        }   

        var command = GenerateTestWorkspaceDefinition(
            workspaceAcronym, new List<string>()
            {
                       TerraformTemplate.NewProjectTemplate,
                       TerraformTemplate.NewProjectTemplate,
                       TerraformTemplate.NewProjectTemplate
            });

        var module = new TerraformTemplate(TerraformTemplate.NewProjectTemplate,
            TerraformStatus.CreateRequested, DateTime.UtcNow);

        await _terraformService.CopyTemplateAsync(module.Name, command);
        await _terraformService.ExtractVariables(module.Name, command);
        await _terraformService.ExtractBackendConfig(workspaceAcronym);

        var moduleDestinationPath = DirectoryUtils.GetProjectPath(_resourceProvisionerConfiguration, workspaceAcronym);
        return Directory
            .GetFiles(moduleDestinationPath, "*.*", SearchOption.TopDirectoryOnly).Length;
    }

    internal static string GenerateWorkspaceAcronym()
    {
        return $"{Guid.NewGuid().ToString().Replace("-", "")[..8]}";
    }
    
    internal static WorkspaceDefinition GenerateTestWorkspaceDefinition(string workspaceAcronym, List<string> terraformTemplates, bool withUsers = true)
    {
        return new WorkspaceDefinition
        {
            Templates = terraformTemplates
                .Select(s => new TerraformTemplate(s, TerraformStatus.CreateRequested, DateTime.UtcNow))
                .ToList(),
            Workspace = GenerateTestTerraformWorkspace(workspaceAcronym, withUsers, TestingWorkspace.Version),
            RequestingUserEmail = RequestingUser,
            ResourceGroupName = ResourceGroup,
            AppData = new WorkspaceAppData()            
        };
    }

    public const string CBR_ID = "5678";

    internal static TerraformWorkspace GenerateTestTerraformWorkspace(string workspaceAcronym, bool withUsers = true, string version = "latest")
    {
        if (!withUsers)
        {
            return new TerraformWorkspace
            {
                Acronym = workspaceAcronym,
                SSCCBRID = CBR_ID,
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
            SSCCBRID = CBR_ID,
            Version = version
        };
    }

    internal static TerraformTemplate GenerateTerraformTemplate(string template)
    {
        return new TerraformTemplate(template, TerraformStatus.CreateRequested, DateTime.UtcNow);
    }
}