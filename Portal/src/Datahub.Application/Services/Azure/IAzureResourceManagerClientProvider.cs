using Azure.ResourceManager;

namespace Datahub.Application.Services.Azure
{
    public interface IAzureResourceManagerClientProvider
    {
        public void Initialize(string tenantId, string clientId, string clientSecret);
        public ArmClient GetClient();
    }
}