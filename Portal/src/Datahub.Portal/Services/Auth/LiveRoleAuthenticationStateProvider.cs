using System.Security.Claims;
using Datahub.Application.Authentication;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Context;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

namespace Datahub.Portal.Services.Auth;

/// <summary>
/// Keeps a server-side Blazor circuit's workspace claims synchronized with the database.
/// </summary>
public sealed class LiveRoleAuthenticationStateProvider(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    RoleClaimTransformer roleClaimTransformer,
    IUserRoleChangeNotifier roleChangeNotifier,
    ILogger<LiveRoleAuthenticationStateProvider> logger)
    : AuthenticationStateProvider, IHostEnvironmentAuthenticationStateProvider, IDisposable
{
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private Task<AuthenticationState>? _authenticationStateTask;
    private IDisposable? _subscription;
    private int _disposed;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return _authenticationStateTask
            ?? throw new InvalidOperationException("Authentication state has not been supplied by the host environment.");
    }

    public void SetAuthenticationState(Task<AuthenticationState> authenticationStateTask)
    {
        ArgumentNullException.ThrowIfNull(authenticationStateTask);
        _authenticationStateTask = authenticationStateTask;
        _ = InitializeAsync(authenticationStateTask);
    }

    private async Task InitializeAsync(Task<AuthenticationState> authenticationStateTask)
    {
        try
        {
            var authenticationState = await authenticationStateTask;
            if (authenticationState.User.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var portalUserId = await ResolvePortalUserIdAsync(authenticationState.User, _disposeCancellation.Token);
            if (!portalUserId.HasValue || _disposeCancellation.IsCancellationRequested)
            {
                return;
            }

            _subscription?.Dispose();
            _subscription = roleChangeNotifier.Subscribe(portalUserId.Value, RefreshAuthenticationStateAsync);
            await RefreshAuthenticationStateAsync(_disposeCancellation.Token);
        }
        catch (OperationCanceledException) when (_disposeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unable to initialize live workspace-role refresh for the current circuit.");
        }
    }

    private async Task RefreshAuthenticationStateAsync(CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var currentStateTask = _authenticationStateTask;
            if (currentStateTask is null)
            {
                return;
            }

            var currentState = await currentStateTask;
            var refreshedPrincipal = await roleClaimTransformer.RefreshAsync(currentState.User);
            var refreshedState = new AuthenticationState(refreshedPrincipal);
            _authenticationStateTask = Task.FromResult(refreshedState);
            NotifyAuthenticationStateChanged(_authenticationStateTask);
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<int?> ResolvePortalUserIdAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var entraObjectId = principal.FindFirstValue(ClaimConstants.ObjectId);
        if (!string.IsNullOrWhiteSpace(entraObjectId))
        {
            return await context.PortalUsers
                .AsNoTracking()
                .Where(user => user.EntraUser != null && user.EntraUser.GraphGuid == entraObjectId)
                .Select(user => (int?)user.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var externalSubject = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(externalSubject))
        {
            return null;
        }

        return await context.PortalUsers
            .AsNoTracking()
            .Where(user => user.ExternalUser != null && user.ExternalUser.ExternalSubject == externalSubject)
            .Select(user => (int?)user.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _subscription?.Dispose();
        _disposeCancellation.Cancel();
        _disposeCancellation.Dispose();
    }
}
