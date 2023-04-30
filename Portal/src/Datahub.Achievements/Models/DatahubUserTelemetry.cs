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
    public int AddOrIncrementEventMetric(string eventName, int value = 1)
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
    
    public int AddOrSetEventMetric(string eventName, int value)
    {
        var metric = EventMetrics.FirstOrDefault(e => e.Name == eventName);

        if (metric is null)
        {
            metric = new DatahubTelemetryEventMetric { Name = eventName, Value = value };
            EventMetrics.Add(metric);
        }
        
        metric.Value = value;

        return metric.Value;
    }
    
    
    public int AddOrSetTelemetryEventKeepMax(string eventName, int value)
    {
        var metric = EventMetrics.FirstOrDefault(e => e.Name == eventName);

        if (metric is null)
        {
            metric = new DatahubTelemetryEventMetric { Name = eventName, Value = value };
            EventMetrics.Add(metric);
        }
        
        metric.Value = Math.Max(value, metric.Value);

        return metric.Value;
    }
}
