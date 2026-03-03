using System;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services.Notification;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services.Notification;

public class UserAccessNotificationServiceTests
{
    [Test]
    public async Task NotifyAccessRegrantedAsync_SendsToUser()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var ctx = new DatahubProjectDBContext(options);

        var user = new PortalUser
        {
            Id = 1,
            Email = "user@test.gc.ca",
            DisplayName = "Test User",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };

        ctx.PortalUsers.Add(user);

        await ctx.SaveChangesAsync();

        var mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        mockFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new DatahubProjectDBContext(options));

        var notifyMock = new Mock<IGCNotifyService>();
        notifyMock
            .Setup(n => n.SendUserAccessRegrantedNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new UserAccessNotificationService(mockFactory.Object, notifyMock.Object);

        await service.NotifyAccessRegrantedAsync(new UserLockStatus
        {
            PortalUserId = user.Id,
            UserName = user.DisplayName,
        }, "Admin User");

        notifyMock.Verify(n => n.SendUserAccessRegrantedNotification("user@test.gc.ca", "Test User", "all workspaces", "Admin User"), Times.Once);
    }
}
