using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Application.Authentication;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using Datahub.Portal.Services.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.FeatureManagement;
using Microsoft.Identity.Web;
using NSubstitute;
using Xunit;

namespace Datahub.Tests.Portal;

public class LiveRoleAuthenticationStateProviderTests
{
    [Fact]
    public async Task MatchingRoleChangeRefreshesCircuitAndDisposalUnsubscribes()
    {
        var options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using (var context = new SqlServerDatahubContext(options))
        {
            var portalUser = new PortalUser
            {
                Id = 42,
                Email = "user@example.com",
                DisplayName = "User"
            };
            portalUser.EntraUser = new EntraUser { GraphGuid = "entra-id", PortalUser = portalUser };
            context.PortalUsers.Add(portalUser);
            await context.SaveChangesAsync();
        }

        var project = new Datahub_Project { Project_Acronym_CD = "TEST" };
        var guestRole = Role(Project_Role.RoleNames.Guest, RoleConstants.GUEST_ROLE);
        var adminRole = Role(Project_Role.RoleNames.Admin, RoleConstants.ADMIN_ROLE);
        var currentRoles = ImmutableList.Create<(Project_Role, Datahub_Project)>((guestRole, project));
        var authManager = Substitute.For<IServiceAuthManager>();
        authManager.GetFreshEntraUserAuthorizations("entra-id").Returns(_ => currentRoles);
        authManager.IsUserCbrOwner(Arg.Any<string>()).Returns(false);

        var featureManager = Substitute.For<IFeatureManagerSnapshot>();
        featureManager.IsEnabledAsync(Arg.Any<string>()).Returns(false);
        var transformer = new RoleClaimTransformer(
            authManager,
            new DatahubPortalConfiguration { AzureAd = new() { TenantId = "tenant-id" } },
            featureManager,
            NullLogger<RoleClaimTransformer>.Instance);
        var notifier = new TestRoleChangeNotifier();
        using var provider = new LiveRoleAuthenticationStateProvider(
            new TestDbContextFactory(options),
            transformer,
            notifier,
            NullLogger<LiveRoleAuthenticationStateProvider>.Instance);
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimConstants.ObjectId, "entra-id"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Role, RoleConstants.TRUSTED_ENTRA_LOGIN)
        ], "test");

        var initialRefresh = NextAuthenticationStateChange(provider);
        provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
        var initialState = await initialRefresh.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(initialState.User.IsInRole($"TEST{RoleConstants.GUEST_SUFFIX}"));

        currentRoles = ImmutableList.Create<(Project_Role, Datahub_Project)>((adminRole, project));
        var roleRefresh = NextAuthenticationStateChange(provider);
        await notifier.NotifyAsync(42);
        var refreshedState = await roleRefresh.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(refreshedState.User.IsInRole($"TEST{RoleConstants.ADMIN_SUFFIX}"));
        Assert.False(refreshedState.User.IsInRole($"TEST{RoleConstants.GUEST_SUFFIX}"));

        provider.Dispose();
        Assert.False(notifier.HasSubscribers(42));
    }

    private static Task<AuthenticationState> NextAuthenticationStateChange(AuthenticationStateProvider provider)
    {
        var completion = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        AuthenticationStateChangedHandler? handler = null;
        handler = async stateTask =>
        {
            provider.AuthenticationStateChanged -= handler;
            completion.TrySetResult(await stateTask);
        };
        provider.AuthenticationStateChanged += handler;
        return completion.Task;
    }

    private static Project_Role Role(Project_Role.RoleNames role, string name) => new()
    {
        Id = (int)role,
        Name = name,
        Description = name
    };

    private sealed class TestDbContextFactory(DbContextOptions<SqlServerDatahubContext> options)
        : IDbContextFactory<DatahubProjectDBContext>
    {
        public DatahubProjectDBContext CreateDbContext() => new SqlServerDatahubContext(options);

        public Task<DatahubProjectDBContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<DatahubProjectDBContext>(new SqlServerDatahubContext(options));
    }

    private sealed class TestRoleChangeNotifier : IUserRoleChangeNotifier
    {
        private readonly Dictionary<int, List<Func<CancellationToken, Task>>> _handlers = new();

        public IDisposable Subscribe(int portalUserId, Func<CancellationToken, Task> handler)
        {
            if (!_handlers.TryGetValue(portalUserId, out var handlers))
            {
                handlers = new();
                _handlers[portalUserId] = handlers;
            }
            handlers.Add(handler);
            return new Subscription(() => handlers.Remove(handler));
        }

        public bool HasSubscribers(int portalUserId) =>
            _handlers.TryGetValue(portalUserId, out var handlers) && handlers.Count > 0;

        public Task NotifyAsync(int portalUserId) =>
            Task.WhenAll(_handlers.GetValueOrDefault(portalUserId, []).Select(handler => handler(CancellationToken.None)));

        private sealed class Subscription(Action unsubscribe) : IDisposable
        {
            public void Dispose() => unsubscribe();
        }
    }
}
