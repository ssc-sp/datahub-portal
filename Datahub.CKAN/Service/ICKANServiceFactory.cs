namespace Datahub.CKAN.Service
{
    public interface ICKANServiceFactory
    {
        ICKANService CreateService();
        bool IsStaging();
    }
}
