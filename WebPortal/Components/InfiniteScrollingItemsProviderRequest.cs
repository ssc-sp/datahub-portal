namespace Datahub.Portal.Components;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public sealed class InfiniteScrollingItemsProviderRequest
{
    public InfiniteScrollingItemsProviderRequest(long startIndex, CancellationToken cancellationToken)
    {
        StartIndex = startIndex;
        CancellationToken = cancellationToken;
    }

    public long StartIndex { get; }
    public CancellationToken CancellationToken { get; }
}

public delegate Task<IEnumerable<T>> ItemsProviderRequestDelegate<T>(InfiniteScrollingItemsProviderRequest request);