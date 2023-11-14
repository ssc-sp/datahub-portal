namespace Datahub.Functions.Providers
{
    public interface IDateProvider
    {
        
        public DateTime Now => DateTime.Now;
        public DateTime Today => DateTime.Today;

        public int[] ProjectNotificationDays();
        
        public int ProjectDeletionDay();

        public int[] UserInactivityNotificationDays();

        public int UserInactivityLockedDay();

        public int UserInactivityDeletionDay();
    }
}