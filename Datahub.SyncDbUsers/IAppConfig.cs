namespace NRCanDataHub
{
    public interface IAppConfig
    {
        string GetConnectionString();
        string GetConnStringTemplate();
        
    }
}
