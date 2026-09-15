namespace Datahub.Infrastructure.Services.Cost;

internal sealed class CostManagementQueryGate(TimeSpan minimumInterval)
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private DateTimeOffset _nextQueryAt = DateTimeOffset.MinValue;

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var delay = _nextQueryAt - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            _nextQueryAt = DateTimeOffset.UtcNow.Add(minimumInterval);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
