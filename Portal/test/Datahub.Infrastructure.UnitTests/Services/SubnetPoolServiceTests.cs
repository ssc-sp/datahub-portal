using Datahub.Application.Configuration;
using Datahub.Application.Exceptions;
using Datahub.Application.Services;
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

        _sut = NewService(CooldownMinutes);
    }

    private sealed class InMemoryContextFactory(DbContextOptions<SqlServerDatahubContext> options)
        : IDbContextFactory<DatahubProjectDBContext>
    {
        public DatahubProjectDBContext CreateDbContext() => new SqlServerDatahubContext(options);
    }

    private SubnetPoolService NewService(int cooldownMinutes)
    {
        var configuration = new DatahubPortalConfiguration();
        configuration.Hosting.ProtectedBSubnetGroupCooldownMinutes = cooldownMinutes;
        return new SubnetPoolService(configuration, new InMemoryContextFactory(_options));
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
        _sut = NewService(5);

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
    public async Task Usage_ShowsStatusAndWorkspaceOfEachGroup()
    {
        await SeedGroupsAsync(1, 2, 3, 4);
        var recentlyDeletedUtc = DateTime.UtcNow.AddMinutes(-10);
        await AssignAsync(1, NewProject("LIVE"));
        await AssignAsync(2, NewProject("RECENT", recentlyDeletedUtc));
        await AssignAsync(3, NewProject("OLD", DateTime.UtcNow.AddDays(-30)));

        await using var ctx = NewContext();
        var vnetId = (await ctx.VNets.SingleAsync()).Id;
        var groupBySubnetId = await ctx.Subnets.ToDictionaryAsync(s => s.Id, s => s.SubnetGroup);

        var usage = (await _sut.GetSubnetGroupUsageAsync(vnetId))
            .ToDictionary(u => groupBySubnetId[u.Key], u => u.Value);

        Assert.Multiple(() =>
        {
            Assert.That(usage[1], Is.EqualTo(new SubnetGroupUsage(SubnetGroupStatus.InUse, "LIVE")));
            Assert.That(usage[2].Status, Is.EqualTo(SubnetGroupStatus.CoolingDown));
            Assert.That(usage[2].WorkspaceAcronym, Is.EqualTo("RECENT"));
            Assert.That(usage[2].AvailableAtUtc, Is.EqualTo(recentlyDeletedUtc.AddMinutes(CooldownMinutes)));
            Assert.That(usage[3], Is.EqualTo(new SubnetGroupUsage(SubnetGroupStatus.Available)));
            Assert.That(usage[4], Is.EqualTo(new SubnetGroupUsage(SubnetGroupStatus.Available)));
        });
    }

    [Test]
    public async Task Usage_OnlyIncludesGroupsOfTheVNet()
    {
        await SeedGroupsAsync(1);
        await SeedGroupsAsync(2);

        await using var ctx = NewContext();
        var firstVNetId = (await ctx.VNets.OrderBy(v => v.Id).FirstAsync()).Id;

        var usage = await _sut.GetSubnetGroupUsageAsync(firstVNetId);

        Assert.That(usage, Has.Count.EqualTo(1));
    }

    private async Task<int> SubnetIdOfGroupAsync(int group)
    {
        await using var ctx = NewContext();
        return (await ctx.Subnets.SingleAsync(s => s.SubnetGroup == group)).Id;
    }

    private async Task SaveProjectAsync(Datahub_Project project)
    {
        await using var ctx = NewContext();
        ctx.Projects.Add(project);
        await ctx.SaveChangesAsync();
    }

    private async Task<List<string>> HoldersOfGroupAsync(int group)
    {
        await using var ctx = NewContext();
        return await ctx.WorkspaceSubnets
            .Where(ws => ws.Subnet.SubnetGroup == group)
            .Select(ws => ws.Project.Project_Acronym_CD)
            .ToListAsync();
    }

    [Test]
    public async Task Attach_AssignsAvailableGroupToWorkspace()
    {
        await SeedGroupsAsync(1);
        await SaveProjectAsync(NewProject("PBW1"));

        await _sut.AttachSubnetGroupAsync(await SubnetIdOfGroupAsync(1), " pbw1 ");

        Assert.That(await HoldersOfGroupAsync(1), Is.EqualTo(new[] { "PBW1" }));
    }

    [Test]
    public async Task Attach_Rejects_GroupInUse()
    {
        await SeedGroupsAsync(1);
        await AssignAsync(1, NewProject("LIVE"));
        await SaveProjectAsync(NewProject("PBW1"));

        await Assert.ThatAsync(async () => await _sut.AttachSubnetGroupAsync(await SubnetIdOfGroupAsync(1), "PBW1"),
            Throws.InvalidOperationException.With.Message.Contains("not available"));
    }

    [Test]
    public async Task Attach_Rejects_UnknownOrDeletedWorkspace()
    {
        await SeedGroupsAsync(1);
        await SaveProjectAsync(NewProject("GONE", DateTime.UtcNow.AddDays(-1)));
        var subnetId = await SubnetIdOfGroupAsync(1);

        Assert.Multiple(async () =>
        {
            await Assert.ThatAsync(() => _sut.AttachSubnetGroupAsync(subnetId, "NOPE"),
                Throws.InvalidOperationException.With.Message.Contains("was not found"));
            await Assert.ThatAsync(() => _sut.AttachSubnetGroupAsync(subnetId, "GONE"),
                Throws.InvalidOperationException.With.Message.Contains("is deleted"));
        });
    }

    [Test]
    public async Task Attach_Rejects_WorkspaceThatAlreadyHasAGroup()
    {
        await SeedGroupsAsync(1, 2);
        await AssignAsync(1, NewProject("PBW1"));

        await Assert.ThatAsync(async () => await _sut.AttachSubnetGroupAsync(await SubnetIdOfGroupAsync(2), "PBW1"),
            Throws.InvalidOperationException.With.Message.Contains("already has subnet group 1"));
    }

    [Test]
    public async Task Release_MakesGroupAvailable()
    {
        await SeedGroupsAsync(1);
        await AssignAsync(1, NewProject("RECENT", DateTime.UtcNow.AddMinutes(-10)));
        var subnetId = await SubnetIdOfGroupAsync(1);

        await _sut.ReleaseSubnetGroupAsync(subnetId);

        await using var ctx = NewContext();
        var usage = await _sut.GetSubnetGroupUsageAsync((await ctx.VNets.SingleAsync()).Id);
        Assert.That(usage[subnetId].Status, Is.EqualTo(SubnetGroupStatus.Available));
    }

    private async Task<int> VNetIdAsync()
    {
        await using var ctx = NewContext();
        return (await ctx.VNets.SingleAsync()).Id;
    }

    private async Task<List<(int Group, string Name)>> GroupsAsync()
    {
        await using var ctx = NewContext();
        return (await ctx.Subnets.OrderBy(s => s.SubnetGroup).ToListAsync())
            .Select(s => (s.SubnetGroup, s.SubnetName)).ToList();
    }

    [Test]
    public async Task Range_AddsMissingGroups_WithGeneratedNames()
    {
        await SeedGroupsAsync(1, 2);

        var result = await _sut.UpdateSubnetGroupRangeAsync(await VNetIdAsync(), 1, 4);

        Assert.Multiple(async () =>
        {
            Assert.That(result, Is.EqualTo((2, 0)));
            Assert.That(await GroupsAsync(), Is.EqualTo(new[]
            {
                (1, "fsdh-dbr_*-1-snet"), (2, "fsdh-dbr_*-2-snet"), (3, "fsdh-dbr_*-3-snet"), (4, "fsdh-dbr_*-4-snet")
            }));
        });
    }

    [Test]
    public async Task Range_ShiftsRange()
    {
        await SeedGroupsAsync(1, 2, 3, 4, 5, 6);

        var result = await _sut.UpdateSubnetGroupRangeAsync(await VNetIdAsync(), 3, 8);

        Assert.Multiple(async () =>
        {
            Assert.That(result, Is.EqualTo((2, 2)));
            Assert.That((await GroupsAsync()).Select(g => g.Group), Is.EqualTo(new[] { 3, 4, 5, 6, 7, 8 }));
        });
    }

    [Test]
    public async Task Range_RemovesAvailableGroupsOutside_WithTheirHistory()
    {
        await SeedGroupsAsync(1, 2, 3, 4);
        await AssignAsync(4, NewProject("OLD", DateTime.UtcNow.AddDays(-30)));

        var result = await _sut.UpdateSubnetGroupRangeAsync(await VNetIdAsync(), 1, 2);

        await using var ctx = NewContext();
        Assert.Multiple(async () =>
        {
            Assert.That(result, Is.EqualTo((0, 2)));
            Assert.That((await GroupsAsync()).Select(g => g.Group), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(await ctx.WorkspaceSubnets.CountAsync(), Is.Zero);
        });
    }

    [Test]
    public async Task Range_Refuses_WhenAGroupOutsideIsInUseOrCoolingDown()
    {
        await SeedGroupsAsync(1, 2, 3, 4, 5);
        await AssignAsync(4, NewProject("LIVE"));
        await AssignAsync(5, NewProject("RECENT", DateTime.UtcNow.AddMinutes(-10)));
        var vnetId = await VNetIdAsync();

        await Assert.ThatAsync(() => _sut.UpdateSubnetGroupRangeAsync(vnetId, 1, 3),
            Throws.InvalidOperationException.With.Message.Contains("Release subnet group(s) 4, 5"));
        Assert.That(await GroupsAsync(), Has.Count.EqualTo(5), "Nothing should change");
    }

    [TestCase(0, 3)]
    [TestCase(5, 2)]
    [TestCase(1, SubnetPoolService.MaxRangeSize + 1)]
    public async Task Range_RejectsInvalidRange(int first, int last)
    {
        await SeedGroupsAsync(1);
        var vnetId = await VNetIdAsync();

        await Assert.ThatAsync(() => _sut.UpdateSubnetGroupRangeAsync(vnetId, first, last), Throws.InvalidOperationException);
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
