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
        // EXP
        public const string UserLogin = "user_login";
        public const string UserOpenDatabricks = "user_click_databricks_link";
        public const string UserViewProjectNotMemberOf = "user_view_project_not_member_of";
        public const string UserRecentLink = "user_recent_link";
        public const string UserToggleCulture = "user_toggle_culture";
        
        
        // PRJ
        public const string UserSentInvite = "user_sent_invite";
        public const string UserAcceptedInvite = "user_accepted_invite";
        public const string UserUploadFile = "user_upload_file";
        public const string UserShareFile = "user_share_file";
        public const string UserDownloadFile = "user_download_file";
        public const string UserDeleteFile = "user_delete_file";
        public const string UserCreateFolder = "user_create_folder";


        
            
    }

}