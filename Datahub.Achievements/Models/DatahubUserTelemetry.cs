using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Achievements.Models;

public class DatahubUserTelemetry
{
    public string? UserId { get; set; }

    public List<DatahubTelemetryEventMetric> EventMetrics { get; set; } = new();
    
    public int GetEventMetric(string eventName)
    {
        var metric = EventMetrics.FirstOrDefault(e => e.Name == eventName);

        if (metric is null)
        {
            metric = new DatahubTelemetryEventMetric { Name = eventName, Value = 0 };
            EventMetrics.Add(metric);
        }

        return metric.Value;
    }
    public int AddOrIncrementEventMetric(string eventName, int value)
    {
        var metric = EventMetrics.FirstOrDefault(e => e.Name == eventName);

        if (metric is null)
        {
            metric = new DatahubTelemetryEventMetric { Name = eventName, Value = value };
            EventMetrics.Add(metric);
        }
        else
        {
            metric.Value += value;
        }

        return metric.Value;
    }

    public struct TelemetryEvents
    {
        public const string UserLogin = "user_login";
        public const string UserSentInvite = "user_sent_invite";
    }

}