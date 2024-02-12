namespace Datahub.CKAN.Service;

public interface ICKANServiceFactory
{
    ICKANService CreateService();
    ICKANService CreateService(string apiKey);
    bool IsStaging();
}