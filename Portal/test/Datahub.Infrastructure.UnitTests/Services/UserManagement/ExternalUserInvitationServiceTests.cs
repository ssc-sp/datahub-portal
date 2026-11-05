using Datahub.Application.Services.Notification;
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
