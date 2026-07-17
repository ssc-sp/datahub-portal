using System;
using System.Collections.Generic;
using System.Text;

namespace Datahub.Core.Configuration
{
    public interface IServiceBusConfiguration
    {
        public string ServiceBusHost { get; }
        public string ServiceBusObjectID { get; }
    }
}
