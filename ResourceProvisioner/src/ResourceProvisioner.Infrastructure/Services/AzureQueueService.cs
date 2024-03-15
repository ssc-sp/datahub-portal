using MassTransit;
using ResourceProvisioner.Domain.Common;
using ResourceProvisioner.Infrastructure.Common;
using ResourceProvisioner.Infrastructure.Helpers;

namespace ResourceProvisioner.Infrastructure.Services
{
    public class AzureQueueService
    {
        private readonly IPublishEndpoint _publishEndpoint;
        public AzureQueueService(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task PublishMessageAsync(PongMessage messageContent)
        {
            var message = new ForwardableAdapter<PongMessage>(messageContent);
            await _publishEndpoint.Publish<IForwardableMessage>(message);
        }
    }
}
