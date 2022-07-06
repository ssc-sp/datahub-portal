namespace Datahub.Portal.Components;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class InfiniteScrollingItemsProviderRequest<T>
{
    public InfiniteScrollingItemsProviderRequest(List<T> items, CancellationToken cancellationToken)
    {
        Items = items;
        CancellationToken = cancellationToken;
    }

    public List<T> Items { get; }
    public CancellationToken CancellationToken { get; }
}

public delegate Task<IEnumerable<T>> ItemsProviderRequestDelegate<T>(InfiniteScrollingItemsProviderRequest<T> request);