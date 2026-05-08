using Datahub.Core.Model.Context;
using Datahub.Core.Model.Subscriptions;
using Datahub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services;

[TestFixture]
public class SubnetPoolServiceTests
{
    // The suffix must match the private constant in SubnetPoolService.
    private const string AppSubnetSuffix = "-app-snet";
    private const string TestSubscriptionId = "sub-test-001";
    private const string TestVNetArmId =
        "/subscriptions/sub-test-001/resourceGroups/rg-test/providers/Microsoft.Network/virtualNetworks/vnet-test";
    private const int TestProjectId = 42;

    private Mock<IDbContextFactory<DatahubProjectDBContext>> _mockFactory = null!;
    private DbContextOptions<SqlServerDatahubContext> _options = null!;

    [SetUp]
    public void Setup()
    {
        // Use a unique in-memory database name per test so each test starts clean.
        _options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var ctx = new SqlServerDatahubContext(_options);
        ctx.Database.EnsureCreated();

        _mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        _mockFactory
            .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
            .ReturnsAsync(() => new SqlServerDatahubContext(_options));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static DatahubAzureSubscription CreateSubscription(string subscriptionId) =>
        new()
        {
            TenantId = "tenant-001",
            SubscriptionId = subscriptionId,
            SubscriptionName = $"Test Subscription ({subscriptionId})"
        };

    private static VNet CreateVNet(DatahubAzureSubscription subscription) =>
        new()
        {
            VNetId = TestVNetArmId,
            VNetName = "vnet-test",
            Subscription = subscription
        };

    /// <summary>
    /// Seeds one subnet group (8 subnets) into the in-memory DB and returns them.
    /// The last subnet in the group has the AppServiceSubnetSuffix.
    /// </summary>
    private async Task<List<Subnet>> SeedSubnetGroupAsync(
        int subnetGroup,
        DatahubAzureSubscription? subscription = null)
    {
        await using var ctx = new SqlServerDatahubContext(_options);

        var sub = subscription ?? CreateSubscription(TestSubscriptionId);
        var vnet = CreateVNet(sub);
        ctx.VNets.Add(vnet);

        var subnets = Enumerable.Range(1, 8)
            .Select(i => new Subnet
            {
                SubnetName = i == 8
                    ? $"subnet-group{subnetGroup}-{i}{AppSubnetSuffix}"  // App Service subnet
                    : $"subnet-group{subnetGroup}-{i}-snet",
                SubnetGroup = subnetGroup,
                VNet = vnet
            })
            .ToList();

        ctx.Subnets.AddRange(subnets);
        await ctx.SaveChangesAsync();

        return subnets;
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    [Test]
    public async Task ClaimOrGet_ReturnsExistingSubnet_WhenWorkspaceAlreadyAssigned()
    {
        // Arrange
        var subnets = await SeedSubnetGroupAsync(subnetGroup: 1);
        var appSubnet = subnets.First(s => s.SubnetName.EndsWith(AppSubnetSuffix));

        await using (var ctx = new SqlServerDatahubContext(_options))
        {
            ctx.WorkspaceSubnets.AddRange(subnets.Select(s => new WorkspaceSubnet
            {
                ProjectId = TestProjectId,
                SubnetId = s.Id
            }));
            await ctx.SaveChangesAsync();
        }

        var sut = new SubnetPoolService(_mockFactory.Object);

        // Act
        var result = await sut.ClaimOrGetAppServiceSubnetIdAsync(TestProjectId, TestSubscriptionId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain(appSubnet.SubnetName));
        Assert.That(result, Does.Contain(TestVNetArmId));
    }

    [Test]
    public async Task ClaimOrGet_ClaimsEntireSubnetGroup_WhenNoAssignmentExists()
    {
        // Arrange
        var subnets = await SeedSubnetGroupAsync(subnetGroup: 1);

        var sut = new SubnetPoolService(_mockFactory.Object);

        // Act
        var result = await sut.ClaimOrGetAppServiceSubnetIdAsync(TestProjectId, TestSubscriptionId);

        // Assert — returns a non-null ARM ID containing the app-service subnet name
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain(AppSubnetSuffix));

        // Assert — all 8 subnets in the group are now assigned to the workspace
        await using var ctx = new SqlServerDatahubContext(_options);
        var assigned = await ctx.WorkspaceSubnets
            .Where(ws => ws.ProjectId == TestProjectId)
            .ToListAsync();

        Assert.That(assigned, Has.Count.EqualTo(subnets.Count),
            "All subnets in the group should be assigned to the workspace");
    }

    [Test]
    public async Task ClaimOrGet_ReturnsNull_WhenNoSubnetsAvailable()
    {
        // Arrange — seed one group but assign it entirely to a different workspace
        var subnets = await SeedSubnetGroupAsync(subnetGroup: 1);
        const int otherProjectId = 99;

        await using (var ctx = new SqlServerDatahubContext(_options))
        {
            ctx.WorkspaceSubnets.AddRange(subnets.Select(s => new WorkspaceSubnet
            {
                ProjectId = otherProjectId,
                SubnetId = s.Id
            }));
            await ctx.SaveChangesAsync();
        }

        var sut = new SubnetPoolService(_mockFactory.Object);

        // Act
        var result = await sut.ClaimOrGetAppServiceSubnetIdAsync(TestProjectId, TestSubscriptionId);

        // Assert
        Assert.That(result, Is.Null,
            "Should return null when all subnet groups are already claimed");
    }

    [Test]
    public async Task ClaimOrGet_IgnoresSubnetsInDifferentSubscription()
    {
        // Arrange — seed subnets under a DIFFERENT subscription
        await SeedSubnetGroupAsync(subnetGroup: 1,
            subscription: CreateSubscription("sub-other-999"));

        var sut = new SubnetPoolService(_mockFactory.Object);

        // Act
        var result = await sut.ClaimOrGetAppServiceSubnetIdAsync(TestProjectId, TestSubscriptionId);

        // Assert
        Assert.That(result, Is.Null,
            "Should not claim subnets from a different Azure subscription");
    }

    [Test]
    public async Task ClaimOrGet_ClaimsNextFreeGroup_WhenFirstGroupAlreadyTaken()
    {
        // Arrange — seed group 1 (claimed) and group 2 (free)
        var group1 = await SeedSubnetGroupAsync(subnetGroup: 1);
        var group2 = await SeedSubnetGroupAsync(subnetGroup: 2);
        const int otherProjectId = 77;

        await using (var ctx = new SqlServerDatahubContext(_options))
        {
            ctx.WorkspaceSubnets.AddRange(group1.Select(s => new WorkspaceSubnet
            {
                ProjectId = otherProjectId,
                SubnetId = s.Id
            }));
            await ctx.SaveChangesAsync();
        }

        var sut = new SubnetPoolService(_mockFactory.Object);

        // Act
        var result = await sut.ClaimOrGetAppServiceSubnetIdAsync(TestProjectId, TestSubscriptionId);

        // Assert — got a result from the free group 2
        Assert.That(result, Is.Not.Null);

        // Only group 2 subnets should be assigned to our workspace
        await using var ctx2 = new SqlServerDatahubContext(_options);
        var assigned = await ctx2.WorkspaceSubnets
            .Include(ws => ws.Subnet)
            .Where(ws => ws.ProjectId == TestProjectId)
            .ToListAsync();

        Assert.That(assigned, Has.Count.EqualTo(group2.Count));
        Assert.That(assigned.All(ws => ws.Subnet.SubnetGroup == 2), Is.True,
            "Should only assign subnets from group 2, not group 1");
    }
}
