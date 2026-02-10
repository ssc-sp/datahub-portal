using System;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Application.Services;
using Datahub.Application.Services.Notification;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services.Notification;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services.Notification;

public class UserAccessNotificationServiceTests
{
    [Test]
    public async Task NotifyAccessRegrantedAsync_SendsToUserAndAdmins()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using var ctx = new DatahubProjectDBContext(options);

        var project = new Datahub_Project
        {
            Project_ID = 10,
            Project_Acronym_CD = "VTA",
            Data_Sensitivity = ClassificationType.Unclassified
        };

        var user = new PortalUser
        {
            Id = 1,
            Email = "user@test.gc.ca",
            DisplayName = "Test User",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };

        var admin = new PortalUser
        {
            Id = 2,
            Email = "admin@test.gc.ca",
            DisplayName = "Admin User",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };

        var lead = new PortalUser
        {
            Id = 3,
            Email = "lead@test.gc.ca",
            DisplayName = "Lead User",
            EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
        };

        ctx.Projects.Add(project);
        ctx.PortalUsers.AddRange(user, admin, lead);
        ctx.UserRolesLinks.AddRange(
            new UserRoleLinks
            {
                PortalUser = admin,
                PortalUserId = admin.Id,
                Project = project,
                Project_ID = project.Project_ID,
                RoleId = (int)Project_Role.RoleNames.Admin,
                Role = new Project_Role { Id = (int)Project_Role.RoleNames.Admin, Name = "Admin", Description = "Admin" }
            },
            new UserRoleLinks
            {
                PortalUser = lead,
                PortalUserId = lead.Id,
                Project = project,
                Project_ID = project.Project_ID,
                RoleId = (int)Project_Role.RoleNames.WorkspaceLead,
                Role = new Project_Role { Id = (int)Project_Role.RoleNames.WorkspaceLead, Name = "Workspace Lead", Description = "Lead" }
            }
        );

        await ctx.SaveChangesAsync();

        var mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        mockFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new DatahubProjectDBContext(options));

        var notifyMock = new Mock<IGCNotifyService>();
        notifyMock
            .Setup(n => n.SendUserAccessRegrantedNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var service = new UserAccessNotificationService(mockFactory.Object, notifyMock.Object);

        await service.NotifyAccessRegrantedAsync(new UserLockStatus
        {
            PortalUserId = user.Id,
            UserName = user.DisplayName,
            WorkspaceId = project.Project_ID,
            WorkspaceAcronym = project.Project_Acronym_CD
        });

        notifyMock.Verify(n => n.SendUserAccessRegrantedNotification("user@test.gc.ca", "Test User", "VTA"), Times.Once);
        notifyMock.Verify(n => n.SendUserAccessRegrantedNotification("admin@test.gc.ca", "Test User", "VTA"), Times.Once);
        notifyMock.Verify(n => n.SendUserAccessRegrantedNotification("lead@test.gc.ca", "Test User", "VTA"), Times.Once);
    }
}
