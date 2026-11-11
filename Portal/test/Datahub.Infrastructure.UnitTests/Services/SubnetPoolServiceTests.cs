using Datahub.Application.Configuration;
using Datahub.Application.Exceptions;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Subscriptions;
using Datahub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.UnitTests.Services;

[TestFixture]
public class SubnetPoolServiceTests
{
    private const int CooldownMinutes = 90;

    private DbContextOptions<SqlServerDatahubContext> _options = null!;
    private SubnetPoolService _sut = null!;

    [SetUp]
    public void Setup()
    {
        // A unique in-memory database per test so each test starts clean.
        _options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var configuration = new DatahubPortalConfiguration();
        configuration.Hosting.ProtectedBSubnetGroupCooldownMinutes = CooldownMinutes;
        _sut = new SubnetPoolService(configuration);
    }

    private SqlServerDatahubContext NewContext() => new(_options);

    private static Datahub_Project NewProject(string acronym, DateTime? deletedUtc = null) => new()
    {
        Project_Acronym_CD = acronym,
        Project_Name = acronym,
        Deleted_DT = deletedUtc
    };

    /// <summary>Seeds one subnet record per group number.</summary>
    private async Task SeedGroupsAsync(params int[] groups)
    {
        await using var ctx = NewContext();
        var vnet = new VNet
        {
            VNetId = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet",
            VNetName = "vnet",
            Subscription = new DatahubAzureSubscription
            {
                TenantId = "tenant", SubscriptionId = "sub", SubscriptionName = "Test subscription"
            }
        };
        ctx.Subnets.AddRange(groups.Select(g => new Subnet { SubnetName = $"group-{g}", SubnetGroup = g, VNet = vnet }));
        await ctx.SaveChangesAsync();
    }

    /// <summary>Assigns a group to a saved workspace.</summary>
    private async Task AssignAsync(int group, Datahub_Project project)
    {
        await using var ctx = NewContext();
        var subnet = await ctx.Subnets.SingleAsync(s => s.SubnetGroup == group);
        ctx.WorkspaceSubnets.Add(new WorkspaceSubnet { Project = project, Subnet = subnet });
        await ctx.SaveChangesAsync();
    }

    private async Task<int> ClaimForNewWorkspaceAsync(string acronym = "NEW")
    {
        await using var ctx = NewContext();
        var project = NewProject(acronym);
        ctx.Projects.Add(project);
        var subnet = await _sut.ClaimSubnetGroupAsync(ctx, project);
        await ctx.SaveChangesAsync();
        return subnet.SubnetGroup;
    }

    [Test]
    public async Task Claim_ReturnsLowestGroup_WhenNoneAssigned()
    {
        await SeedGroupsAsync(3, 1, 2);

        Assert.That(await ClaimForNewWorkspaceAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Claim_SkipsGroupHeldByLiveWorkspace()
    {
        await SeedGroupsAsync(1, 2);
        await AssignAsync(1, NewProject("LIVE"));

        Assert.That(await ClaimForNewWorkspaceAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Claim_SkipsGroupOfWorkspaceDeletedWithinCooldown()
    {
        await SeedGroupsAsync(1, 2);
        await AssignAsync(1, NewProject("RECENT", DateTime.UtcNow.AddMinutes(-(CooldownMinutes - 10))));

        Assert.That(await ClaimForNewWorkspaceAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Claim_ReusesGroupOfWorkspaceDeletedBeforeCooldown()
    {
        await SeedGroupsAsync(1, 2);
        await AssignAsync(1, NewProject("OLD", DateTime.UtcNow.AddMinutes(-(CooldownMinutes + 10))));

        Assert.That(await ClaimForNewWorkspaceAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Claim_SkipsGroupHeldByLiveWorkspace_EvenWithOlderDeletedHolder()
    {
        await SeedGroupsAsync(1, 2);
        await AssignAsync(1, NewProject("OLD", DateTime.UtcNow.AddDays(-30)));
        await AssignAsync(1, NewProject("LIVE"));

        Assert.That(await ClaimForNewWorkspaceAsync(), Is.EqualTo(2));
    }

    [Test]
    public async Task Claim_UsesConfiguredCooldown()
    {
        var configuration = new DatahubPortalConfiguration();
        configuration.Hosting.ProtectedBSubnetGroupCooldownMinutes = 5;
        _sut = new SubnetPoolService(configuration);

        await SeedGroupsAsync(1, 2);
        await AssignAsync(1, NewProject("OLD", DateTime.UtcNow.AddMinutes(-10)));

        Assert.That(await ClaimForNewWorkspaceAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task Claim_Throws_WhenNoGroupAvailable()
    {
        await SeedGroupsAsync(1);
        await AssignAsync(1, NewProject("LIVE"));

        await Assert.ThatAsync(() => ClaimForNewWorkspaceAsync(), Throws.TypeOf<NoSubnetGroupAvailableException>());
    }

    [Test]
    public async Task Claim_AddsAssignmentWithoutSaving()
    {
        await SeedGroupsAsync(1);

        await using (var ctx = NewContext())
        {
            var project = NewProject("NEW");
            ctx.Projects.Add(project);
            await _sut.ClaimSubnetGroupAsync(ctx, project);

            Assert.That(ctx.ChangeTracker.Entries<WorkspaceSubnet>().Single().State, Is.EqualTo(EntityState.Added));
            // Not saved: the caller saves the assignment together with the workspace.
        }

        await using var check = NewContext();
        Assert.That(await check.WorkspaceSubnets.CountAsync(), Is.Zero);
    }
}
