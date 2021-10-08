using System;
using System.Threading;
using System.Threading.Tasks;

namespace Datahub.Core.Utils
{
    /// <summary>
    /// Query throttler utility class
    /// </summary>
    public class QueryThrottler<T> where T: IComparable<T>
    {
        private readonly TimeSpan _delay;
        private readonly Func<T, Task> _callback;
        private T _query;

        /// <summary>
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
        /// <returns></returns>
        public Task SetQuery(T query)
        {
            return Task.Run(() =>
            {
                if (query != null)
                {
                    lock (this)
                    {
                        _query = query;
                    }

                    Thread.Sleep(_delay);

                    lock (this)
                    {
                        if (!query.Equals(_query))
                            return;
                    }

                    _ = _callback.Invoke(query);
                }
            });
        }
    }
}
