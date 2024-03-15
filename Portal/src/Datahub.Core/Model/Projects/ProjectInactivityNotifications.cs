namespace Datahub.Core.Model.Projects
{
    public class ProjectInactivityNotifications
    {
        public int Project_ID { get; set; }
        public Datahub_Project Project { get; set; }
        public DateTime NotificationDate { get; set; }
        public int DaysBeforeDeletion { get; set; }
        public string SentTo { get; set; }
    }
}