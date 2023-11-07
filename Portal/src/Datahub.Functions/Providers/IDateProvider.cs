namespace Datahub.Functions.Providers
{
    public interface IDateProvider
    {
        
        public DateTime Now => DateTime.Now;
        public DateTime Today => DateTime.Today;

        public int[] NotificationDays();
        
        public int DeletionDay();
    }
}