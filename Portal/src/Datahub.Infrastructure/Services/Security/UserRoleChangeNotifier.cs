using System.Collections.Concurrent;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Security;

/// <summary>
/// Polls the database-backed role change feed and notifies active portal circuits.
/// </summary>
public sealed class UserRoleChangeNotifier(
    IDbContextFactory<DatahubProjectDBContext> dbContextFactory,
    IServiceAuthManager serviceAuthManager,
    ILogger<UserRoleChangeNotifier> logger)
    : BackgroundService, IUserRoleChangeNotifier
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);
    private const int BatchSize = 1_000;
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<Guid, Func<CancellationToken, Task>>> _subscriptions = new();
    private long _lastProcessedId;

    public IDisposable Subscribe(int portalUserId, Func<CancellationToken, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var subscriptionId = Guid.NewGuid();
        var subscriptions = _subscriptions.GetOrAdd(portalUserId, _ => new());
        subscriptions[subscriptionId] = handler;
        return new Subscription(() => RemoveSubscription(portalUserId, subscriptionId));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await InitializeWatermarkAsync(stoppingToken);
                break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to initialize the workspace role change feed. Initialization will be retried.");
                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        using var timer = new PeriodicTimer(PollInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await PollForChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unable to process workspace role changes. The change feed will be retried.");
            }
        }
    }

    internal async Task InitializeWatermarkAsync(CancellationToken cancellationToken)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        _lastProcessedId = await context.PortalUserRoleChanges
            .AsNoTracking()
            .MaxAsync(change => (long?)change.Id, cancellationToken) ?? 0;
    }

    internal async Task PollForChangesAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var changes = await context.PortalUserRoleChanges
                .AsNoTracking()
                .Where(change => change.Id > _lastProcessedId)
                .OrderBy(change => change.Id)
                .Select(change => new { change.Id, change.PortalUserId })
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (changes.Count == 0)
            {
                return;
            }

            serviceAuthManager.InvalidateWorkspaceAuthorizationCache();
            foreach (var portalUserId in changes.Select(change => change.PortalUserId).Distinct())
            {
                await NotifyUserAsync(portalUserId, cancellationToken);
            }

            _lastProcessedId = changes[^1].Id;
            if (changes.Count < BatchSize)
            {
                return;
            }
        }
    }

    private async Task NotifyUserAsync(int portalUserId, CancellationToken cancellationToken)
    {
        if (!_subscriptions.TryGetValue(portalUserId, out var subscriptions))
        {
            return;
        }

        await Task.WhenAll(subscriptions.Values.Select(handler => handler(cancellationToken)));
    }

    private void RemoveSubscription(int portalUserId, Guid subscriptionId)
    {
        if (!_subscriptions.TryGetValue(portalUserId, out var subscriptions))
        {
            return;
        }

        subscriptions.TryRemove(subscriptionId, out _);
        if (subscriptions.IsEmpty)
        {
            _subscriptions.TryRemove(new KeyValuePair<int, ConcurrentDictionary<Guid, Func<CancellationToken, Task>>>(portalUserId, subscriptions));
        }
    }

    private sealed class Subscription(Action unsubscribe) : IDisposable
    {
        private Action? _unsubscribe = unsubscribe;

        public void Dispose() => Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
    }
}
