namespace Datahub.Functions.Providers
{
    public interface IDateProvider
    {
        DateTime Now { get; set; }
        DateTime Today { get; set; }

        int[] ProjectNotificationDays();
        int ProjectSoftDeletionDay();
        int ProjectHardDeletionDay();
        int[] UserInactivityNotificationDays();
        int UserInactivityLockedDay();
        int UserInactivityDeletionDay();
    }
}
