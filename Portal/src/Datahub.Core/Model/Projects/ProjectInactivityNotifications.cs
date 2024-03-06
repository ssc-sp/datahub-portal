namespace Datahub.Core.Model.Projects
{
    public class ProjectInactivityNotifications
    {
        public int ProjectID { get; set; }
        public DatahubProject Project { get; set; }
        public DateTime NotificationDate { get; set; }
        public int DaysBeforeDeletion { get; set; }
        public string SentTo { get; set; }
    }
}