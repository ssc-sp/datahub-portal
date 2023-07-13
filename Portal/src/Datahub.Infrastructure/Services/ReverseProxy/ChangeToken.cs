using Microsoft.Extensions.Primitives;

namespace Datahub.Infrastructure.Services.ReverseProxy;

internal class ChangeToken : IChangeToken
{
    public bool ActiveChangeCallbacks => false;

    public bool HasChanged => true;

    public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
    {
#pragma warning disable CS8603 // Possible null reference return.
        return null;
#pragma warning restore CS8603 // Possible null reference return.
    }
}


