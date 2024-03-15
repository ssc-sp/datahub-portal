using MassTransit;
using ResourceProvisioner.Domain.Common;

namespace ResourceProvisioner.Infrastructure.Services
{
    public class ForwardingConsumer: IConsumer<IForwardableMessage>
    {
        private readonly AzureServiceBusForwarder _forwarder;
        public ForwardingConsumer(AzureServiceBusForwarder forwarder)
        {
            _forwarder = forwarder;
        }
        public async Task Consume(ConsumeContext<IForwardableMessage> context)
        {
            await _forwarder.ForwardMessageAsync(context.Message);
        }
    }
}
