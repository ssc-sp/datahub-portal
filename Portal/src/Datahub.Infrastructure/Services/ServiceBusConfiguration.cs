using Azure.Messaging.ServiceBus;
using Datahub.Application.Configuration;
using Datahub.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Infrastructure.Services
{
    public class ServiceBusConfiguration(DatahubPortalConfiguration configuration) : IServiceBusConfiguration
    {
        public string ServiceBusHost
        {
            get
            {
                var host = ServiceBusConnectionStringProperties.Parse(configuration.DatahubServiceBus.ConnectionString);
                return host.Endpoint.Host;
            }
        }

        public string ServiceBusObjectID => configuration.DatahubServiceBus.ObjectId;
    }

    public class NoServiceBusConfiguration : IServiceBusConfiguration
    {
        public string ServiceBusHost => null!;
        public string ServiceBusObjectID => null!;
    }
}
