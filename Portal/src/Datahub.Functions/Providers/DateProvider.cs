namespace Datahub.Functions.Providers
{
    public class DateProvider : IDateProvider
    {
        private readonly AzureConfig _config;

        public DateProvider(AzureConfig config)
        {
            _config = config;
        }
        
        public int[] NotificationDays()
        {
            return ParseProjectInactivityNotificationDays(_config.ProjectInactivityNotificationDays ?? "7,2");
        }

        public int DeletionDay()
        {
            return ParseProjectInactivityDeletionDay(_config.ProjectInactivityDeletionDays ?? "30");
        }
        
        private int[] ParseProjectInactivityNotificationDays(string days)
        {
            return days.Split(",").Select(int.Parse).ToArray();
        }
        
        private int ParseProjectInactivityDeletionDay(string deletionDay)
        {
            return int.Parse(deletionDay);
        }
    }
}