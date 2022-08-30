namespace Datahub.Achievements.Models;

public class DatahubUserTelemetry
{
    public string? UserId { get; set; }

    public Dictionary<string, int> EventMetrics { get; set; } = new();
    
    public int GetEventMetric(string eventName)
    {
        if (!EventMetrics.ContainsKey(eventName))
        {
            EventMetrics[eventName] = 0;
        }
        return EventMetrics[eventName];
    }
    public int AddOrIncrementEventMetric(string eventName, int value)
    {
        if (EventMetrics.ContainsKey(eventName))
        {
            EventMetrics[eventName] += value;
        }
        else
        {
            EventMetrics.Add(eventName, value);
        }

        return EventMetrics[eventName];
    }

    public struct TelemetryEvents
    {
        public const string UserLogin = "user_login";
        public const string UserSentInvite = "user_sent_invite";
    }

}