using System;

namespace NRCan.Datahub.Shared.Services
{
    public interface IDatahubAuditingService
    {
        void TrackEvent(string eventName, params (string key, string value)[] properties);
        void TrackDataEvent(string eventName, string eventData, params (string key, string value)[] properties);
        void TrackException(Exception exception, params (string key, string value)[] properties);
    }
}
