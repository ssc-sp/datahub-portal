using Datahub.Application.Services.Security;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;

using static Datahub.Infrastructure.Services.Storage.CloudStorageHelpers;

namespace Datahub.Infrastructure.Services.Storage
{
	public class CloudStorageManagerFactory
    {
        public CloudStorageManagerFactory(ILoggerFactory loggerFactory, IKeyVaultUserService keyVaultUserService)
        {
            _logFactory = loggerFactory;
            this.keyVaultUserService = keyVaultUserService;
            _logger = loggerFactory.CreateLogger<CloudStorageManagerFactory>();
        }

        private readonly ILogger<CloudStorageManagerFactory> _logger;
        private readonly ILoggerFactory _logFactory;
        private readonly IKeyVaultUserService keyVaultUserService;

        public string KeyVaultSecretName(int id) => $"cloud-storage-{id}";

        public async Task<ICloudStorageManager?> CreateCloudStorageManager(string acronym, ProjectCloudStorage stg)
        {
            var connectionData = await keyVaultUserService.GetAllSecrets(stg, acronym);
            if (connectionData is null)
            {
                _logger.LogWarning("Could not find connection data for cloud storage provider with id {0}", stg.Id);
                return null;
            }
            return CreateCloudStorageManager(stg.Id, stg.Provider, stg.Name, connectionData, stg.Enabled);
        }

        public async Task<IDictionary<string,string>> GetConnectionSecrets(ProjectCloudStorage pcs, string? acronym = null)
        {
            if (acronym is null)
                acronym = pcs.Project.Project_Acronym_CD;
            if (pcs.Id == 0)
                return CreateNewStorageProperties();
            IDictionary<string, string>? existingSecrets = null;
            try
            {
                existingSecrets = await keyVaultUserService.GetAllSecrets(pcs, acronym);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,"Could not find connection data for cloud storage provider with id {0}", pcs.Id);
            }
            if (existingSecrets?.Count > 0)
            {
                return existingSecrets;
            }
            else
            {
                return CreateNewStorageProperties();
            }
        }

        public static IDictionary<string, string> CreateNewStorageProperties()
        {
            var connectionData = new Dictionary<string, string>();
            connectionData[AZ_AccountName] = string.Empty;
            connectionData[AZ_AccountKey] = string.Empty;
            connectionData[AWS_AccesKeyId] = string.Empty;
            connectionData[AWS_AccessKeySecret] = string.Empty;
            connectionData[AWS_Region] = string.Empty;
            connectionData[AWS_BucketName] = string.Empty;
            connectionData[GCP_ProjectId] = string.Empty;
            connectionData[GCP_Json] = string.Empty;
            return connectionData;
        }

        public ICloudStorageManager? CreateTestCloudStorageManager(CloudStorageProviderType providerType, IDictionary<string, string> connectionData) =>
            CreateCloudStorageManager(default, providerType.ToString(), "test", connectionData, true);

        private ICloudStorageManager? CreateCloudStorageManager(int id, string provider, string name, IDictionary<string,string> connectionData, bool enabled)
        {
            if (provider == CloudStorageProviderType.Azure.ToString())
            {

                var stAccountName = string.IsNullOrEmpty(name) ? connectionData[AZ_AccountName] : name;

                ICloudStorageManager storageManager = enabled ?
                    new AzureCloudStorageManager(connectionData[AZ_AccountName], connectionData[AZ_AccountKey], stAccountName) :
                    new DisabledCloudStorageManager(CloudStorageProviderType.Azure, stAccountName);

                return storageManager;
            }
            else if (provider == CloudStorageProviderType.AWS.ToString())
            {

                var stAccountName = string.IsNullOrEmpty(name) ? connectionData[AWS_BucketName] : name;

                ICloudStorageManager storageManager = enabled ?
                    new AWSCloudStorageManager(stAccountName, connectionData[AWS_AccesKeyId], connectionData[AWS_AccessKeySecret],
                        connectionData[AWS_Region], connectionData[AWS_BucketName]) :
                    new DisabledCloudStorageManager(CloudStorageProviderType.AWS, stAccountName);

                return storageManager;
            }
            else if (provider == CloudStorageProviderType.GCP.ToString())
            {

                var stAccountName = string.IsNullOrEmpty(name) ? connectionData[GCP_ProjectId] : name;

                ICloudStorageManager storageManager = enabled ?
                    new GoogleCloudStorageManager(_logFactory, connectionData[GCP_ProjectId], connectionData[GCP_Json], stAccountName) :
                    new DisabledCloudStorageManager(CloudStorageProviderType.GCP, stAccountName);
                
                return storageManager;
            }
            else
            {
                _logger.LogWarning("Invalid provider type {0} for cloud storage provider with id {1}", provider, id);
                return null;
            }
        }
    }
}
