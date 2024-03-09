using Azure.Identity;
using Azure.ResourceManager;
using Datahub.Application.Services.Azure;
using Microsoft.Extensions.Logging;

namespace Datahub.Infrastructure.Services.Azure
{
    public class AzureResourceManagerClientProvider : IAzureResourceManagerClientProvider
    {
        private ClientSecretCredential _credentials;
        private readonly ILogger<AzureResourceManagerClientProvider> _logger;

        public AzureResourceManagerClientProvider(ILogger<AzureResourceManagerClientProvider> logger)
        {
            _logger = logger;
        }

        public void Initialize(string tenantId, string clientId, string clientSecret)
        {
            _credentials = new ClientSecretCredential(tenantId, clientId, clientSecret);
        }

        public ArmClient GetClient()
        {
            if (_credentials == null)
            {
                _logger.LogError("ArmClient provider not initialized");
                throw new Exception("ArmClient provider not initialized. Run Initialize() first.");
            }

            var client = new ArmClient(_credentials);
            return client;
        }
    }
}