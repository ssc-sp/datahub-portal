namespace Datahub.Functions.Providers
{
    public class DateProvider : IDateProvider
    {
        private readonly AzureConfig _config;

        public DateProvider(AzureConfig config)
        {
            _config = config;
        }
        
        public int[] ProjectNotificationDays()
        {
            return ParseProjectInactivityNotificationDays(_config.ProjectInactivityNotificationDays ?? "7,2");
        }

        public int ProjectDeletionDay()
        {
            return ParseProjectInactivityDeletionDay(_config.ProjectInactivityDeletionDays ?? "30");
        }
        
        public int[] UserInactivityNotificationDays()
        {
            return ParseUserInactivityNotificationDays(_config.UserInactivityNotificationDays ?? "7,2");
        }
        
        public int UserInactivityLockedDay()
        {
            return ParseUserInactivityLockedDay(_config.UserInactivityLockedDays ?? "30");
        }
        
        public int UserInactivityDeletionDay()
        {
            return ParseUserInactivityDeletionDay(_config.UserInactivityDeletionDays ?? "60");
        }
        
        private int[] ParseProjectInactivityNotificationDays(string days)
        {
            return days.Split(",").Select(int.Parse).ToArray();
        }
        
        private int ParseProjectInactivityDeletionDay(string deletionDay)
        {
            return int.Parse(deletionDay);
        }
        
        private int[] ParseUserInactivityNotificationDays(string days)
        {
            return days.Split(",").Select(int.Parse).ToArray();
        }
        
        private int ParseUserInactivityLockedDay(string lockedDay)
        {
            return int.Parse(lockedDay);
        }
        
        private int ParseUserInactivityDeletionDay(string deletionDay)
        {
            return int.Parse(deletionDay);
        }
    }
}