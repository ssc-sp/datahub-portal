using System;

namespace Datahub.Core.Model.Projects
{
    public class Project_Inactivity_Notifications
    {
        public int Project_ID { get; set; }
        public DateTime NotificationDate { get; set; }
        public int DaysBeforeDeletion { get; set; }
        public string SentTo { get; set; }
    }
}