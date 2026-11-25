using Datahub.Application.Services.Notification;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services.UserManagement;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services.UserManagement;

public class ExternalUserInvitationServiceTests
{
    [Test]
    public async Task CompleteInvitationAsync_ReactivatesUserOnlyInInvitingWorkspace()
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var requestedExpiry = DateTimeOffset.UtcNow.AddMonths(3);

        await using (var seedContext = new DatahubProjectDBContext(options))
        {
            var inviter = new PortalUser { Id = 10, Email = "inviter@test.gc.ca", DisplayName = "Inviter" };
            var portalUser = new PortalUser { Id = 11, Email = "external@example.com", DisplayName = "External User" };
            var externalUser = new ExternalUser
            {
                Id = 1,
                FirstName = "External",
                LastName = "User",
                Organization = "Test Org",
                ExternalSubject = "existing-subject",
                UserExpiryDate = DateTimeOffset.UtcNow.AddDays(-1),
                UserDeactivatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                DeactivationReason = "No longer required",
                DeactivatedByUser = inviter,
                PortalUserId = portalUser.Id,
                PortalUser = portalUser
            };
            portalUser.ExternalUser = externalUser;

            var targetProject = new Datahub_Project
            {
                Project_ID = 1,
                Project_Acronym_CD = "TARGET",
                Project_Name = "Target",
                Project_Name_Fr = "Cible"
            };
            var oldProject = new Datahub_Project
            {
                Project_ID = 2,
                Project_Acronym_CD = "OLD",
                Project_Name = "Old",
                Project_Name_Fr = "Ancien"
            };
            var removedRole = new Project_Role
            {
                Id = (int)Project_Role.RoleNames.Removed,
                Name = "Remove User",
                Description = "Removed"
            };
            var externalRole = new Project_Role
            {
                Id = (int)Project_Role.RoleNames.WebApp,
                Name = "Web Application Access",
                Description = "Web app",
                IsExternalRole = true
            };

            seedContext.AddRange(inviter, portalUser, externalUser, targetProject, oldProject, removedRole, externalRole);
            seedContext.UserRolesLinks.AddRange(
                new UserRoleLinks { ProjectUser_ID = 1, Project_ID = targetProject.Project_ID, PortalUserId = portalUser.Id, RoleId = externalRole.Id },
                new UserRoleLinks { ProjectUser_ID = 2, Project_ID = oldProject.Project_ID, PortalUserId = portalUser.Id, RoleId = externalRole.Id });
            seedContext.ExternalUserRequests.Add(new WorkspaceInvitation
            {
                User = externalUser,
                InvitedBy = inviter,
                Project = oldProject,
                InvitationToken = Guid.NewGuid(),
                InvitedEmail = portalUser.Email,
                InvitationExpiry = DateTimeOffset.UtcNow.AddDays(1),
                InvitationCode = "ABCD-EFGH-JKLM",
                InvitationRationale_EN = "Older invitation",
                Request_DT = DateTimeOffset.UtcNow.AddDays(-1),
                Requested_Role = externalRole
            });
            await seedContext.SaveChangesAsync();
        }

        var factory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        factory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new DatahubProjectDBContext(options));
        var authManager = new Mock<IServiceAuthManager>();
        var service = new ExternalUserInvitationService(
            factory.Object,
            Mock.Of<IGCNotifyService>(),
            authManager.Object,
            NullLogger<ExternalUserInvitationService>.Instance);

        WorkspaceInvitation invitation;
        await using (var invitationContext = new DatahubProjectDBContext(options))
        {
            var inviter = await invitationContext.PortalUsers.SingleAsync(user => user.Id == 10);
            invitation = await service.CreateReactivationInvitationAsync(
                1,
                "TARGET",
                "external@example.com",
                "Reactivation Invitation",
                (int)Project_Role.RoleNames.WebApp,
                requestedExpiry,
                inviter,
                _ => ("https://example.com/en", "https://example.com/fr"));
        }

        await using (var pendingContext = new DatahubProjectDBContext(options))
        {
            var pendingUser = await pendingContext.ExternalUsers.SingleAsync(user => user.Id == 1);
            Assert.That(pendingUser.UserDeactivatedAt, Is.Not.Null);
            Assert.That(pendingUser.UserExpiryDate, Is.Not.EqualTo(requestedExpiry));
        }

        var completed = await service.CompleteInvitationAsync(
            invitation.InvitationToken,
            invitation.InvitationCode,
            "existing-subject");

        Assert.That(completed, Is.True);
        await using var verifyContext = new DatahubProjectDBContext(options);
        var reactivatedUser = await verifyContext.ExternalUsers.SingleAsync(user => user.Id == 1);
        Assert.Multiple(() =>
        {
            Assert.That(reactivatedUser.UserDeactivatedAt, Is.Null);
            Assert.That(reactivatedUser.DeactivationReason, Is.Null);
            Assert.That(reactivatedUser.DeactivatedByUserId, Is.Null);
            Assert.That(reactivatedUser.UserExpiryDate, Is.EqualTo(requestedExpiry));
        });

        var workspaceLinks = await verifyContext.UserRolesLinks.OrderBy(link => link.Project_ID).ToListAsync();
        Assert.Multiple(() =>
        {
            Assert.That(workspaceLinks.Single(link => link.Project_ID == 1).RoleId, Is.EqualTo((int)Project_Role.RoleNames.WebApp));
            Assert.That(workspaceLinks.Single(link => link.Project_ID == 2).RoleId, Is.EqualTo((int)Project_Role.RoleNames.Removed));
        });
        var olderInvitation = await verifyContext.ExternalUserRequests
            .SingleAsync(storedInvitation => storedInvitation.InvitationRationale_EN == "Older invitation");
        Assert.That(olderInvitation.InvitationExpiry, Is.LessThanOrEqualTo(DateTimeOffset.UtcNow));
        authManager.Verify(manager => manager.InvalidateWorkspaceAuthorizationCache(), Times.Once);
    }

    [Test]
    public async Task CreateInvitationAsync_ThrowsInvalidOperationException_WhenProjectDataSensitivityIsProtectedB()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using (var seedCtx = new DatahubProjectDBContext(options))
        {
            var inviter = new PortalUser
            {
                Id = 10,
                Email = "inviter@test.gc.ca",
                DisplayName = "Inviter User",
                EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
            };

            var externalUserPortalUser = new PortalUser
            {
                Id = 11,
                Email = "external@test.gc.ca",
                DisplayName = "External User",
                EntraUser = new EntraUser { GraphGuid = Guid.NewGuid().ToString(), PortalUser = null! }
            };

            var externalUser = new ExternalUser
            {
                Id = 1,
                FirstName = "External",
                LastName = "User",
                Organization = "Test Org",
                UserExpiryDate = DateTimeOffset.UtcNow.AddYears(1),
                PortalUserId = externalUserPortalUser.Id,
                PortalUser = externalUserPortalUser
            };

            seedCtx.PortalUsers.AddRange(inviter, externalUserPortalUser);
            seedCtx.ExternalUsers.Add(externalUser);
            seedCtx.Project_Roles.Add(new Project_Role
            {
                Id = (int)Project_Role.RoleNames.Guest,
                Name = "Guest",
                Description = "Guest"
            });
            seedCtx.Projects.Add(new Datahub_Project
            {
                Project_Acronym_CD = "TEST",
                Project_Name = "Test Project",
                Project_Name_Fr = "Projet de test",
                Data_Sensitivity = ClassificationType.ProtectedB
            });

            await seedCtx.SaveChangesAsync();
        }

        var factory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        factory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new DatahubProjectDBContext(options));

        var service = new ExternalUserInvitationService(
            factory.Object,
            Mock.Of<IGCNotifyService>(),
            Mock.Of<IServiceAuthManager>(),
            NullLogger<ExternalUserInvitationService>.Instance);

        await using (var verifyCtx = new DatahubProjectDBContext(options))
        {
            var inviter = await verifyCtx.PortalUsers.SingleAsync(p => p.Email == "inviter@test.gc.ca");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.CreateInvitationAsync(
                    externalUserId: 1,
                    projectAcronym: "TEST",
                    invitedEmail: "external@test.gc.ca",
                    invitationRationale: "Test invitation",
                    projectRoleId: (int)Project_Role.RoleNames.Guest,
                    inviter: inviter,
                    GetCodeAcceptancePageUrl: _ => ("https://example.com/en", "https://example.com/fr")));

            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Does.Contain("does not allow external user invitations"));
        }
    }
}
