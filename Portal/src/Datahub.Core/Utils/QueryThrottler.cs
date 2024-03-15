namespace Datahub.Core.Utils;

/// <summary>
/// Query throttler utility class
/// </summary>
public class QueryThrottler<T> where T : IComparable<T>
{
    private readonly TimeSpan delay;
    private readonly Func<T, Task> callback;
    private T query;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="delay">Wait delay between callbacks</param>
    /// <param name="callback">Callback lambda</param>
    public QueryThrottler(TimeSpan delay, Func<T, Task> callback)
    {
        this.delay = delay;
        this.callback = callback;
    }

    /// <summary>
    /// Set the query to throttle
    /// </summary>
    /// <param name="query">Query to throttle. Null queries will be ignored.</param>
    /// <returns></returns>
    public Task SetQuery(T query)
    {
        return Task.Run(() =>
        {
            if (query != null)
            {
                lock (this)
                {
                    this.query = query;
                }

                Thread.Sleep(delay);

                lock (this)
                {
                    if (!query.Equals(this.query))
                        return;
                }

                _ = callback.Invoke(query);
            }
        });
    }
}