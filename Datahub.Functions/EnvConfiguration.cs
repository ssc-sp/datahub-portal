namespace NRCanDataHub
{
    public class EnvConfiguration : IAppConfig
    {
        public string GetConnectionString()
        {
            return System.Environment.GetEnvironmentVariable("projectDbConnectionString");
        }

        public string GetConnStringTemplate()
        {
            return System.Environment.GetEnvironmentVariable("dbConnStringTemplate");
        }       
    }
}
