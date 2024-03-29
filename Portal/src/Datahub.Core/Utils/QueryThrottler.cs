﻿namespace Datahub.Core.Utils;

/// <summary>
/// Query throttler utility class
/// </summary>
/// <typeparam name="T">Type of query to throttle</typeparam>
public class QueryThrottler<T>
    where T : IComparable<T>
{
    private readonly TimeSpan delay;
    private readonly Func<T, Task> callback;
    private T query;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryThrottler{T}"/> class.
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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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