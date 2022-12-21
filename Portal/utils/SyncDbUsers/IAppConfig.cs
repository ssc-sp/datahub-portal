namespace SyncDbUsers;

public interface IAppConfig
{
    string GetConnectionString();
    string GetConnStringTemplate();
        
}