using Datahub.Core.Model.Achievements;

namespace Datahub.Core.Model.UserTracking
{
    public class UserInactivityNotifications
    {
        public int User_ID { get; set; }
        public PortalUser User { get; set; }
        public DateTime NotificationDate { get; set; }
        public int DaysBeforeLocked { get; set; }
        public int DaysBeforeDeleted { get; set; }
    }
}