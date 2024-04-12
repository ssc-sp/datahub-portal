using MassTransit;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services;

public class LoggingFilter<T> : IFilter<ConsumeContext<T>> where T : class
{
    private readonly ILogger<LoggingFilter<T>> _logger;
    public LoggingFilter(ILogger<LoggingFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        _logger.LogInformation($"Message: {typeof(T).Name}");
        await next.Send(context);
    }
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("loggingFilter");
    }
}