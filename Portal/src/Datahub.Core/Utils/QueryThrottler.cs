namespace Datahub.Core.Utils;

/// <summary>
/// Query throttler utility class
/// </summary>
/// <typeparam name="T">Type of query to throttle</typeparam>
public class QueryThrottler<T>
    where T : IComparable<T>
{
    private readonly TimeSpan _delay;
    private readonly Func<T, Task> _callback;
    private T _query;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryThrottler{T}"/> class.
    /// Constructor
    /// </summary>
    /// <param name="delay">Wait delay between callbacks</param>
    /// <param name="callback">Callback lambda</param>
    public QueryThrottler(TimeSpan delay, Func<T, Task> callback)
    {
        _delay = delay;
        _callback = callback;
    }

    /// <summary>
    /// Set the query to throttle
    /// </summary>
    /// <param name="query">Query to throttle. Null queries will be ignored.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task SetQuery(T query)
    {
        return Task.Run(() =>
        {
            if (query != null)
            {
                lock (this)
                {
                    this._query = query;
                }

                Thread.Sleep(_delay);

                lock (this)
                {
                    if (!query.Equals(this._query))
                        return;
                }

                _ = _callback.Invoke(query);
            }
        });
    }
}