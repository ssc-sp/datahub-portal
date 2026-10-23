using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Model.Users;
using Datahub.Application;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Datahub.Infrastructure.Services.Security;
using Datahub.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Datahub.Metadata.Model;

namespace Datahub.Tests.ResourceProvisioner;

public class ProjectCreationTests
{
    private Mock<IResourceMessagingService>? _resourceMessagingService;

    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ProjectCreationQueue:ConnectionString"] = "UseDevelopmentStorage=true;",
                ["ProjectCreationQueue:Name"] = "project-creation-test-queue"
            })
            .AddUserSecrets<Datahub.Portal.Startup>(optional: true);

        return builder.Build();
    }

    private static WorkspaceDefinition CreateWorkspaceDefinitionStub() => new()
    {
        Templates = [],
        Workspace = new TerraformWorkspace
        {
            Name = "test-workspace",
            Acronym = "TST",
            SubscriptionId = "subscription-id"
        },
        AppData = new WorkspaceAppData(),
        RequestingUserEmail = "unit-test@example.com",
        ResourceGroupName = "rg-test",
        CBRID = "cbr-test"
    };

    private const string ResourceProvisionerUrl = "https://localhost:7275";
    private static IEnumerable<T> LoadCollectionGeneric<TS,T>(ServiceProvider provider, Func<TS, IEnumerable> loadSource) where TS:DbContext
    {            
        var fac = provider.GetRequiredService<IDbContextFactory<TS>>();
        using var ctx = fac.CreateDbContext();
        return ((loadSource(ctx) as IEnumerable<T>) ?? throw new InvalidOperationException()).ToList();
    }
    private ServiceProvider SetupServices(string? databaseName = null)
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration();
        _resourceMessagingService = new Mock<IResourceMessagingService>();
        _resourceMessagingService
            .Setup(x => x.CreateWorkspaceDefinition(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(CreateWorkspaceDefinitionStub());
        _resourceMessagingService
            .Setup(x => x.SendToTerraformQueue(It.IsAny<WorkspaceDefinition>()))
            .Returns(Task.CompletedTask);
        _resourceMessagingService
            .Setup(x => x.QueueRBACSync(It.IsAny<WorkspaceDefinition>()))
            .Returns(Task.CompletedTask);

        var auditingService = new Mock<IDatahubAuditingService>();
        auditingService
            .Setup(x => x.TrackDataEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuditChangeType>(), It.IsAny<bool>(), It.IsAny<(string Key, string Value)[]>()))
            .Returns(Task.CompletedTask);
        auditingService
            .Setup(x => x.TrackSecurityEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuditChangeType>(), It.IsAny<(string Key, string Value)[]>()))
            .Returns(Task.CompletedTask);
        auditingService
            .Setup(x => x.TrackAdminEvent(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<AuditChangeType>(), It.IsAny<(string Key, string Value)[]>()))
            .Returns(Task.CompletedTask);
        auditingService
            .Setup(x => x.TrackException(It.IsAny<Exception>(), It.IsAny<(string Key, string Value)[]>()))
            .Returns(Task.CompletedTask);
        auditingService
            .Setup(x => x.TrackEvent(It.IsAny<string>(), It.IsAny<(string Key, string Value)[]>()))
            .Returns(Task.CompletedTask);

        var subService = new Mock<IDatahubAzureSubscriptionService>();
        subService.Setup(s => s.NextSubscriptionAsync()).ReturnsAsync(new DatahubAzureSubscription { SubscriptionId = "1234", SubscriptionName = "Test", TenantId = "1234" });
        var catService = new Mock<IDatahubCatalogSearch>();
        var metService = new Mock<IMetadataBrokerService>();
        var verService = new Mock<IWorkspaceVersionService>();
        verService.Setup(x => x.GetLatestVersionAsync()).ReturnsAsync("1.1.1");

        services.AddLogging();
        services.AddMemoryCache();
        var dbName = databaseName ?? "datahubProjects";
        services.AddPooledDbContextFactory<DatahubProjectDBContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddPortalConfiguration(configuration);
        services.AddScoped<IWorkspaceCreationService, WorkspaceCreationService>();
        services.AddSingleton<IWorkspaceVersionService>(verService.Object);
        services.AddSingleton<IServiceAuthManager>(sp =>
            new ServiceAuthManager(
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>(),
                NullLogger<ServiceAuthManager>.Instance));
        services.AddSingleton<IResourceMessagingService>(_resourceMessagingService.Object);
        services.AddSingleton<IDatahubAuditingService>(auditingService.Object);
        services.AddSingleton<IDatahubAzureSubscriptionService>(subService.Object);
        services.AddSingleton<IDatahubCatalogSearch>(catService.Object);
        services.AddSingleton<IMetadataBrokerService>(metService.Object);
        services.AddSingleton<IConfiguration>(configuration);

        var userService = new Mock<IUserInformationService>();
        userService.Setup(x => x.GetCurrentPortalUserAsync()).ReturnsAsync(new PortalUser { Email = "test@test.com" });
        services.AddSingleton<IUserInformationService>(userService.Object);
        return services.BuildServiceProvider();
    }
    
    [Fact (Skip = "Needs to be validated")]
    public async Task GivenListOfProjects_CreateAcronyms()
    {
        var projects = new[]
        {
            "Datahub", "Datahub Core", "Datahub Core Services", "Datahub Core Services API", 
            "Datahub Core Services App", "Datahub Core Services App API", "Datahub Core Services Application",
        };
        var acronyms = new List<string>();
        var projectCreationService = SetupServices().GetRequiredService<IWorkspaceCreationService>();
        foreach (var project in projects)
        {
            var acronym = await projectCreationService.GenerateWorkspaceAcronymAsync(project, acronyms);
            acronyms.Add(acronym);
        }
        Assert.Equal(new[] { "DAT", "DACO", "DCS", "DCSA", "DCSA1", "DCSAA", "DCSA2" }, acronyms);
    }

    [Fact]
    public async Task GivenDatahubProjectWithoutAcronym_CreateResourcesAndAddProject()
    {
        const string projectName = "FSDH Unit Testing";
        const string organization = "Unit Testing";
        var serviceProvider = SetupServices();
        var projectCreationService = serviceProvider.GetRequiredService<IWorkspaceCreationService>();

        var isAdded = await projectCreationService.CreateWorkspaceAsync(projectName, organization);

        Assert.True(isAdded);

        var projects = LoadCollectionGeneric<DatahubProjectDBContext, Datahub_Project>(serviceProvider, d => d.Projects);
        Assert.Single(projects);
        Assert.Equal(projectName, projects.Single().Project_Name);
        _resourceMessagingService!.Verify(x => x.SendToTerraformQueue(It.IsAny<WorkspaceDefinition>()), Times.Once);
    }

    [Fact]
    public async Task GivenProtectedBWorkspace_CreateWorkspace_AddsAvailableSubnetMapping()
    {
        const string projectName = "Protected B Unit Testing";
        const string organization = "Unit Testing";
        const string dbName = "protected-b-test-db";

        var serviceProvider = SetupServices(dbName);
        var projectCreationService = serviceProvider.GetRequiredService<IWorkspaceCreationService>();

        var factory = serviceProvider.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>();
        await using (var db = await factory.CreateDbContextAsync())
        {
            var subscription = new DatahubAzureSubscription
            {
                TenantId = "tenant-123",
                SubscriptionId = "sub-123",
                SubscriptionName = "Test Sub"
            };

            var vnet = new VNet
            {
                VNetId = "/subscriptions/sub-123/resourceGroups/rg-test/providers/Microsoft.Network/virtualNetworks/vnet-test",
                VNetName = "vnet-test",
                Subscription = subscription
            };

            var subnet = new Subnet
            {
                SubnetName = "workspace-subnet-1",
                AddressPrefix = "10.0.0.0/24",
                SubnetGroup = 1,
                VNet = vnet
            };

            db.VNets.Add(vnet);
            db.Subnets.Add(subnet);
            await db.SaveChangesAsync();
        }

        var isAdded = await projectCreationService.CreateWorkspaceAsync(projectName, organization, classification: ClassificationType.ProtectedB);

        Assert.True(isAdded);

        await using (var db = await factory.CreateDbContextAsync())
        {
            var project = await db.Projects.SingleAsync(p => p.Project_Name == projectName);
            var workspaceSubnets = await db.WorkspaceSubnets
                .Include(ws => ws.Subnet)
                .Where(ws => ws.ProjectId == project.Project_ID)
                .ToListAsync();

            Assert.Single(workspaceSubnets);
            Assert.Equal(projectName, project.Project_Name);
            Assert.Equal("workspace-subnet-1", workspaceSubnets.Single().Subnet.SubnetName);
        }
    }
}
