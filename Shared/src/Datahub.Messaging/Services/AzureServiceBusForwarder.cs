﻿using Azure.Messaging.ServiceBus;
using MediatR;

using System.Text;

namespace Datahub.Shared.Messaging;

public class AzureServiceBusForwarder
{
    private readonly ServiceBusClient _client;
    private readonly string _topicName = "TBD";
    public AzureServiceBusForwarder(string connectionString)
    {
        _client = new ServiceBusClient(connectionString);
    }

    public async Task ForwardMessageAsync(IForwardableMessage message)
    {
        var sender = _client.CreateSender(_topicName);
        var serviceBasMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(message.Content));
        await sender.SendMessageAsync(serviceBasMessage);
    }
}
