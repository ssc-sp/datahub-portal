using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using NRCan.Datahub.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRCan.Datahub.Shared
{
    public class DatahubAuditingService : IDatahubAuditingService
    {
        private readonly TelemetryClient _telemetryClient;

        public DatahubAuditingService()
        {
            _telemetryClient = new TelemetryClient(TelemetryConfiguration.CreateDefault());
        }
        
        public void TrackEvent(string eventName, params (string key, string value)[] properties)
        {
            TrackEvent(eventName, MakeDictionary(properties));
        }

        public void TrackDataEvent(string eventName, string eventData, params (string key, string value)[] properties)
        {
            var propDict = MakeDictionary(properties);
            propDict.Add("EventData", eventData);
            TrackEvent(eventName, propDict);
        }

        public void TrackException(Exception exception, params (string key, string value)[] properties)
        {
            _telemetryClient.TrackException(exception, MakeDictionary(properties));
        }

        private void TrackEvent(string eventName, IDictionary<string, string> properties)
        {
            _telemetryClient.TrackEvent(eventName, properties);
        }

        static Dictionary<string, string> MakeDictionary((string key, string value)[] properties)
        {
            return properties.ToDictionary(p => p.key, p => p.value);
        }
    }
}
