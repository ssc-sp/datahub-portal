using Datahub.Application.Configuration;
using Datahub.Application.Exceptions;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Application.Services.Security;
using Datahub.Application.Services.Subscriptions;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Model.Users;
using Datahub.Core.Services.CatalogSearch;
using Datahub.Infrastructure.Services;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests.Services;

/// <summary>
/// Subnet group assignment when a workspace is created.
/// </summary>
[TestFixture]
public class WorkspaceCreationSubnetGroupTests
{
    private DbContextOptions<SqlServerDatahubContext> _options = null!;
    private WorkspaceCreationService _sut = null!;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var factory = new InMemoryContextFactory(_options);

        var userInformationService = Substitute.For<IUserInformationService>();
        userInformationService.GetCurrentPortalUserAsync()
            .Returns(new PortalUser { Id = 1, Email = "lead@example.gc.ca" });

        var subscriptionService = Substitute.For<IDatahubAzureSubscriptionService>();
        subscriptionService.NextSubscriptionAsync()
            .Returns(new DatahubAzureSubscription { Id = 1, TenantId = "tenant", SubscriptionId = "sub", SubscriptionName = "Test subscription" });

        var workspaceVersionService = Substitute.For<IWorkspaceVersionService>();
        workspaceVersionService.GetLatestVersionAsync().Returns("v1.0.0");

        var configuration = new DatahubPortalConfiguration();

        _sut = new WorkspaceCreationService(
            configuration,
            factory,
            Substitute.For<ILogger<WorkspaceCreationService>>(),
            Substitute.For<IServiceAuthManager>(),
            userInformationService,
            workspaceVersionService,
            Substitute.For<IResourceMessagingService>(),
            Substitute.For<IDatahubAuditingService>(),
            subscriptionService,
            Substitute.For<IDatahubCatalogSearch>(),
            Substitute.For<IMetadataBrokerService>(),
            new SubnetPoolService(configuration, factory));
    }

    private sealed class InMemoryContextFactory(DbContextOptions<SqlServerDatahubContext> options)
        : IDbContextFactory<DatahubProjectDBContext>
    {
        public DatahubProjectDBContext CreateDbContext() => new SqlServerDatahubContext(options);
    }

    private async Task SeedGroupAsync(int group)
    {
        await using var ctx = new SqlServerDatahubContext(_options);
        ctx.Subnets.Add(new Subnet
        {
            SubnetName = $"group-{group}",
            SubnetGroup = group,
            VNet = new VNet
            {
                VNetId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet",
                VNetName = "vnet",
                Subscription = new DatahubAzureSubscription { TenantId = "tenant", SubscriptionId = "sub-vnet", SubscriptionName = "VNet subscription" }
            }
        });
        await ctx.SaveChangesAsync();
    }

    [Test]
    public async Task ProtectedBWorkspace_IsSavedWithASubnetGroup()
    {
        await SeedGroupAsync(1);

        await _sut.CreateWorkspaceAsync("Protected workspace", "PBW1", "Org", classification: ClassificationType.ProtectedB);

        await using var ctx = new SqlServerDatahubContext(_options);
        var assignment = await ctx.WorkspaceSubnets.Include(ws => ws.Project).Include(ws => ws.Subnet).SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(assignment.Project.Project_Acronym_CD, Is.EqualTo("PBW1"));
            Assert.That(assignment.Subnet.SubnetGroup, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ProtectedBWorkspace_IsNotSaved_WhenNoSubnetGroupAvailable()
    {
        await Assert.ThatAsync(
            () => _sut.CreateWorkspaceAsync("Protected workspace", "PBW1", "Org", classification: ClassificationType.ProtectedB),
            Throws.TypeOf<NoSubnetGroupAvailableException>());

        await using var ctx = new SqlServerDatahubContext(_options);
        Assert.That(await ctx.Projects.AnyAsync(p => p.Project_Acronym_CD == "PBW1"), Is.False);
    }

    [Test]
    public async Task UnclassifiedWorkspace_DoesNotNeedASubnetGroup()
    {
        await _sut.CreateWorkspaceAsync("Open workspace", "UNC1", "Org");

        await using var ctx = new SqlServerDatahubContext(_options);
        Assert.Multiple(async () =>
        {
            Assert.That(await ctx.Projects.AnyAsync(p => p.Project_Acronym_CD == "UNC1"), Is.True);
            Assert.That(await ctx.WorkspaceSubnets.AnyAsync(), Is.False);
        });
    }
}
