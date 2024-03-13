namespace Datahub.Application.Services.Publishing;

public interface ICKANServiceFactory
{
    ICKANService CreateService();
    ICKANService CreateService(string apiKey);
    bool IsStaging();
}