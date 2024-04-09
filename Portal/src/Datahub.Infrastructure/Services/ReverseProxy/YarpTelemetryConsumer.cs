using System.Collections.Concurrent;
using Yarp.ReverseProxy.Forwarder;
using Yarp.Telemetry.Consumption;

namespace Datahub.Infrastructure.Services.ReverseProxy;

public class YarpTelemetryConsumer : IForwarderTelemetryConsumer
{

    private (DateTime Timestamp, ForwarderError error)? _lastError;

    public (DateTime Timestamp, string errorDescription)? GetLastError() => _lastError is null?null:(_lastError.Value.Timestamp, _lastError.Value.error.ToString());

    private ConcurrentDictionary<(string cluster, string route), DateTime> _lastInvoke = new();

    public List<(string cluster, string route, DateTime timeStamp)> GetLastInvokeStats() => _lastInvoke.Select(e => (e.Key.cluster, e.Key.route, e.Value)).ToList();

    /// Called before forwarding a request.
    public void OnForwarderStart(DateTime timestamp, string destinationPrefix)
    {
        //ignore
        //Console.WriteLine($"Forwarder Telemetry [{timestamp:HH:mm:ss.fff}] => OnForwarderStart :: Destination prefix: {destinationPrefix}");
    }

    /// Called after forwarding a request.
    public void OnForwarderStop(DateTime timestamp, int statusCode)
    {
        //ignore
    }

    /// Called before <see cref="OnForwarderStop(DateTime, int)"/> if forwarding the request failed.
    public void OnForwarderFailed(DateTime timestamp, ForwarderError error)
    {
        _lastError = (timestamp, error);
    }

    /// Called when reaching a given stage of forwarding a request.
    public void OnForwarderStage(DateTime timestamp, ForwarderStage stage)
    {
        //ignore
    }

    /// Called periodically while a content transfer is active.
    public void OnContentTransferring(DateTime timestamp, bool isRequest, long contentLength, long iops, TimeSpan readTime, TimeSpan writeTime)
    {
        //ignore
    }

    /// Called after transferring the request or response content.
    public void OnContentTransferred(DateTime timestamp, bool isRequest, long contentLength, long iops, TimeSpan readTime, TimeSpan writeTime, TimeSpan firstReadTime)
    {
        //ignore
    }

    /// Called before forwarding a request from `ForwarderMiddleware`, therefore is not called for direct forwarding scenarios.
    public void OnForwarderInvoke(DateTime timestamp, string clusterId, string routeId, string destinationId)
    {
        _lastInvoke.TryAdd((clusterId, routeId), timestamp);
        //Console.WriteLine($"Forwarder Telemetry [{timestamp:HH:mm:ss.fff}] => OnForwarderInvoke:: Cluster id: {clusterId}, Route Id: {routeId}, Destination: {destinationId}");
    }
}
