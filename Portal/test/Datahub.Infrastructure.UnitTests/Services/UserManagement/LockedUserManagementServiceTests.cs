using System;
using System.Linq;
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
    public async Task UnlockUserAsync_CreatesUnlockEvent_WhenExternalUserExists()
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
        ctx.ExternalUsers.Add(new ExternalUser
        {
            Id = 77,
            PortalUserId = 1,
            PortalUser = ctx.PortalUsers.Local.First(p => p.Id == 1),
            FirstName = "Test",
            LastName = "User",
            Organization = "Datahub",
            UserExpiryDate = DateTimeOffset.UtcNow.AddDays(10)
        });
        await ctx.SaveChangesAsync();

        var factory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        factory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new DatahubProjectDBContext(options));

        var service = new LockedUserManagementService(factory.Object);

        var result = await service.UnlockUserAsync(1, "Unlocked for testing", 2);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.EventType, Is.EqualTo(ExternalUserLockEventType.Unlocked));
        Assert.That(result.PortalUserId, Is.EqualTo(1));

        await using var verifyCtx = new DatahubProjectDBContext(options);
        var savedEvent = await verifyCtx.ExternalUserLockAuditEvents.FirstOrDefaultAsync();
        Assert.That(savedEvent, Is.Not.Null);
        Assert.That(savedEvent!.EventType, Is.EqualTo(ExternalUserLockEventType.Unlocked));
        Assert.That(savedEvent.PerformedByUserId, Is.EqualTo(2));
        Assert.That(savedEvent.Notes, Does.Contain("ExternalUserId=77"));
        Assert.That(savedEvent.Notes, Does.Contain("ExternalUserEmail=user@test.gc.ca"));
    }

    [Test]
    public void UnlockUserAsync_ThrowsAndDoesNotCreateEvent_WhenExternalUserMissing()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        using (var seedCtx = new DatahubProjectDBContext(options))
        {
            seedCtx.PortalUsers.Add(new PortalUser
            {
                Id = 1,
                Email = "user@test.gc.ca",
                DisplayName = "Test User",
                EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
            });
            seedCtx.PortalUsers.Add(new PortalUser
            {
                Id = 2,
                Email = "admin@test.gc.ca",
                DisplayName = "Admin User",
                EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
            });
            seedCtx.SaveChanges();
        }

        var factory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        factory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new DatahubProjectDBContext(options));

        var service = new LockedUserManagementService(factory.Object);

        Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.UnlockUserAsync(1, "Unlocked for testing", 2));

        using var verifyCtx = new DatahubProjectDBContext(options);
        Assert.That(verifyCtx.ExternalUserLockAuditEvents.Count(), Is.EqualTo(0));
    }
}
