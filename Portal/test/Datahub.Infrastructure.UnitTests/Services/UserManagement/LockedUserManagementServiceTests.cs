using System;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services.UserManagement;

public class LockedUserManagementServiceTests
{
    [Test]
    public async Task UnlockUserAsync_CreatesUnlockEvent()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var ctx = new DatahubProjectDBContext(options);
        ctx.PortalUsers.Add(new PortalUser
        {
            Id = 1,
            Email = "user@test.gc.ca",
            DisplayName = "Test User",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        });
        ctx.PortalUsers.Add(new PortalUser
        {
            Id = 2,
            Email = "admin@test.gc.ca",
            DisplayName = "Admin User",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        });
        await ctx.SaveChangesAsync();

        var factory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        factory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new DatahubProjectDBContext(options));

        var service = new LockedUserManagementService(factory.Object);

        var result = await service.UnlockUserAsync(1, 10, "Unlocked for testing", 2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EventType, Is.EqualTo(LockEventType.Unlocked));
        Assert.That(result.PortalUserId, Is.EqualTo(1));
        Assert.That(result.WorkspaceId, Is.EqualTo(10));

        await using var verifyCtx = new DatahubProjectDBContext(options);
        var savedEvent = await verifyCtx.UserWorkspaceLocks.FirstOrDefaultAsync();
        Assert.That(savedEvent, Is.Not.Null);
        Assert.That(savedEvent!.EventType, Is.EqualTo(LockEventType.Unlocked));
        Assert.That(savedEvent.PerformedByUserId, Is.EqualTo(2));
    }
}
