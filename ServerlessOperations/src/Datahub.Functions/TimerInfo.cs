namespace Datahub.Functions;

public class TimerInfo
{
    public TimerScheduleStatus ScheduleStatus { get; set; } = default!;
    public bool IsPastDue { get; set; }
}

public class TimerScheduleStatus
{
    public DateTime Last { get; set; }
    public DateTime Next { get; set; }
    public DateTime LastUpdated { get; set; }
}
