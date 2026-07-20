using Datahub.Infrastructure.Queues.Messages;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Infrastructure.Services.VirusScan
{
    public interface IVirusScanStatusConsumer
    {
        event Func<VirusScanStatusMessage, Task> OnVirusScanStatusReceived;

        Task Consume(ConsumeContext<VirusScanStatusMessage> context);
    }

    public class VirusScanStatusConsumer : IConsumer<VirusScanStatusMessage>, IVirusScanStatusConsumer
    {
        public event Func<VirusScanStatusMessage, Task> OnVirusScanStatusReceived = null!;

        public async Task Consume(ConsumeContext<VirusScanStatusMessage> context)
        {
            if (OnVirusScanStatusReceived != null)
            {
                await OnVirusScanStatusReceived.Invoke(context.Message);
            }
        }

    }
}
