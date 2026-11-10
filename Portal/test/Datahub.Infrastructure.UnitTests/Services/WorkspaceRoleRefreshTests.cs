using System.Collections.Immutable;
using System.Security.Claims;
using Datahub.Application.Authentication;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Infrastructure.Services.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services;

public class WorkspaceRoleRefreshTests
{
    [Test]
    public async Task SaveChangesAsync_WhenWorkspaceRoleChanges_WritesOneChangeRecord()
    {
        var options = NewOptions();
        await using var context = new SqlServerDatahubContext(options);
        var user = new PortalUser { Email = "user@example.com", DisplayName = "User" };
        var project = new Datahub_Project { Project_Acronym_CD = "TEST" };
        context.AddRange(user, project);
        await context.SaveChangesAsync();

        var link = new UserRoleLinks
        {
            PortalUserId = user.Id,
            Project_ID = project.Project_ID,
            RoleId = (int)Project_Role.RoleNames.Guest
        };
        context.UserRolesLinks.Add(link);
        await context.SaveChangesAsync();

        context.PortalUserRoleChanges.Should().ContainSingle(change =>
            change.PortalUserId == user.Id && change.RoleId == Project_Role.RoleNames.Guest);

        link.IsDataSteward = true;
        await context.SaveChangesAsync();
        context.PortalUserRoleChanges.Should().HaveCount(1);

        link.RoleId = (int)Project_Role.RoleNames.Collaborator;
        await context.SaveChangesAsync();
        context.PortalUserRoleChanges.Should().HaveCount(2);
        context.PortalUserRoleChanges.OrderBy(change => change.Id).Last().RoleId
            .Should().Be(Project_Role.RoleNames.Collaborator);
    }

    [Test]
    public async Task PollForChangesAsync_NotifiesOnlyAffectedUsersAndDeduplicatesBatch()
    {
        var options = NewOptions();
        var factory = new TestDbContextFactory(options);
        var authManager = new Mock<IServiceAuthManager>();
        var notifier = new UserRoleChangeNotifier(factory, authManager.Object, NullLogger<UserRoleChangeNotifier>.Instance);
        await notifier.InitializeWatermarkAsync(CancellationToken.None);

        var affectedNotifications = 0;
        var unrelatedNotifications = 0;
        using var affectedSubscription = notifier.Subscribe(10, _ =>
        {
            affectedNotifications++;
            return Task.CompletedTask;
        });
        using var unrelatedSubscription = notifier.Subscribe(20, _ =>
        {
            unrelatedNotifications++;
            return Task.CompletedTask;
        });

        await using (var context = new SqlServerDatahubContext(options))
        {
            context.PortalUserRoleChanges.AddRange(
                new PortalUserRoleChange { PortalUserId = 10, RoleId = Project_Role.RoleNames.Guest, ChangeDate = DateTime.UtcNow },
                new PortalUserRoleChange { PortalUserId = 10, RoleId = Project_Role.RoleNames.Admin, ChangeDate = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }

        await notifier.PollForChangesAsync(CancellationToken.None);

        affectedNotifications.Should().Be(1);
        unrelatedNotifications.Should().Be(0);
        authManager.Verify(manager => manager.InvalidateWorkspaceAuthorizationCache(), Times.Once);
    }

    [Test]
    public async Task RefreshAsync_ReplacesGeneratedRolesWithoutChangingIdentityProviderClaims()
    {
        var authManager = new Mock<IServiceAuthManager>();
        var guestRole = new Project_Role
        {
            Id = (int)Project_Role.RoleNames.Guest,
            Name = RoleConstants.GUEST_ROLE,
            Description = RoleConstants.GUEST_ROLE
        };
        var adminRole = new Project_Role
        {
            Id = (int)Project_Role.RoleNames.Admin,
            Name = RoleConstants.ADMIN_ROLE,
            Description = RoleConstants.ADMIN_ROLE
        };
        var project = new Datahub_Project { Project_Acronym_CD = "TEST" };
        authManager.Setup(manager => manager.GetEntraUserAuthorizations("entra-id"))
            .ReturnsAsync(ImmutableList.Create<(Project_Role, Datahub_Project)>((guestRole, project)));
        authManager.Setup(manager => manager.GetFreshEntraUserAuthorizations("entra-id"))
            .ReturnsAsync(ImmutableList.Create<(Project_Role, Datahub_Project)>((adminRole, project)));

        var featureManager = new Mock<IFeatureManagerSnapshot>();
        featureManager.Setup(manager => manager.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        var configuration = new DatahubPortalConfiguration { AzureAd = new() { TenantId = "tenant-id" } };
        var transformer = new RoleClaimTransformer(
            authManager.Object,
            configuration,
            featureManager.Object,
            NullLogger<RoleClaimTransformer>.Instance);
        var identityProviderClaim = new Claim("source", "preserved");
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimConstants.ObjectId, "entra-id"),
            new Claim(ClaimTypes.Role, RoleConstants.TRUSTED_ENTRA_LOGIN),
            identityProviderClaim
        ], "test");
        var initialPrincipal = await transformer.TransformAsync(new ClaimsPrincipal(identity));

        var refreshedPrincipal = await transformer.RefreshAsync(initialPrincipal);

        refreshedPrincipal.IsInRole($"TEST{RoleConstants.ADMIN_SUFFIX}").Should().BeTrue();
        refreshedPrincipal.IsInRole($"TEST{RoleConstants.GUEST_SUFFIX}").Should().BeFalse();
        refreshedPrincipal.Claims.Should().ContainSingle(claim => claim.Type == "source" && claim.Value == "preserved");
        refreshedPrincipal.Claims
            .Where(claim => claim.Issuer == RoleClaimTransformer.GENERATED_CLAIM_ISSUER)
            .Select(claim => (claim.Type, claim.Value))
            .Should().OnlyHaveUniqueItems();
    }

    [Test]
    public async Task RefreshAsync_ReplacesExternalUserRolesFromFreshAuthorizations()
    {
        var authManager = new Mock<IServiceAuthManager>();
        var webAppRole = new Project_Role
        {
            Id = (int)Project_Role.RoleNames.WebApp,
            Name = "Web App",
            Description = "Web App",
            IsExternalRole = true
        };
        var storageRole = new Project_Role
        {
            Id = (int)Project_Role.RoleNames.Storage,
            Name = "Storage",
            Description = "Storage",
            IsExternalRole = true
        };
        var project = new Datahub_Project { Project_Acronym_CD = "TEST" };
        authManager.Setup(manager => manager.GetExternalUserAuthorizations("external-id"))
            .ReturnsAsync(ImmutableList.Create<(Project_Role, Datahub_Project)>((webAppRole, project)));
        authManager.Setup(manager => manager.GetFreshExternalUserAuthorizations("external-id"))
            .ReturnsAsync(ImmutableList.Create<(Project_Role, Datahub_Project)>((storageRole, project)));

        var featureManager = new Mock<IFeatureManagerSnapshot>();
        featureManager.Setup(manager => manager.IsEnabledAsync(It.IsAny<string>())).ReturnsAsync(false);
        var transformer = new RoleClaimTransformer(
            authManager.Object,
            new DatahubPortalConfiguration { AzureAd = new() { TenantId = "tenant-id" } },
            featureManager.Object,
            NullLogger<RoleClaimTransformer>.Instance);
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "external-id"),
            new Claim(ClaimTypes.Role, RoleConstants.EXTERNAL_LOGIN),
            new Claim("source", "preserved")
        ], "test"));

        var initialPrincipal = await transformer.TransformAsync(principal);
        var refreshedPrincipal = await transformer.RefreshAsync(initialPrincipal);

        refreshedPrincipal.IsInRole($"TEST{RoleConstants.STORAGE_SUFFIX}").Should().BeTrue();
        refreshedPrincipal.IsInRole($"TEST{RoleConstants.WEBAPP_SUFFIX}").Should().BeFalse();
        refreshedPrincipal.Claims.Should().ContainSingle(claim => claim.Type == "source" && claim.Value == "preserved");
    }

    private static DbContextOptions<SqlServerDatahubContext> NewOptions() =>
        new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private sealed class TestDbContextFactory(DbContextOptions<SqlServerDatahubContext> options)
        : IDbContextFactory<DatahubProjectDBContext>
    {
        public DatahubProjectDBContext CreateDbContext() => new SqlServerDatahubContext(options);

        public Task<DatahubProjectDBContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<DatahubProjectDBContext>(new SqlServerDatahubContext(options));
    }
}
