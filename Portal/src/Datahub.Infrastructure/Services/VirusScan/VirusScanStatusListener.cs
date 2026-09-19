using Datahub.Infrastructure.Queues.Messages;
using MassTransit;
using Org.BouncyCastle.Tsp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Infrastructure.Services.VirusScan
{
    public interface IVirusScanStatusListener
    {
        event Func<VirusScanStatusMessage, Task> OnVirusScanStatusReceived;

        Task Notify(VirusScanStatusMessage context);
    }

    public class VirusScanStatusListener : IVirusScanStatusListener
    {
        public event Func<VirusScanStatusMessage, Task> OnVirusScanStatusReceived = null!;

        public async Task Notify(VirusScanStatusMessage message)
        {
            if (OnVirusScanStatusReceived != null)
            {
                await OnVirusScanStatusReceived.Invoke(message);
            }
        }

    }

    public class VirusScanStatusConsumer(IVirusScanStatusListener listener) : IConsumer<VirusScanStatusMessage>
    {
        public async Task Consume(ConsumeContext<VirusScanStatusMessage> context)
        {
            await listener.Notify(context.Message);
        }


    }

}
