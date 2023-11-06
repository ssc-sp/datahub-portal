using Datahub.Application.Services.Storage;
using Datahub.Core.Model.CloudStorage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services.Storage
{
    public class CloudStorageManagerFactory
    {
        public CloudStorageManagerFactory(ILoggerFactory loggerFactory)
        {
            _logFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<CloudStorageManagerFactory>();
        }

        private readonly ILogger<CloudStorageManagerFactory> _logger;
        private readonly ILoggerFactory _logFactory;

        public ICloudStorageManager? CreateCloudStorageManager(ProjectCloudStorage stg) => 
            CreateCloudStorageManager(stg.Id, stg.Provider, stg.Name, stg.ConnectionData, stg.Enabled);

        public ICloudStorageManager? CreateTestCloudStorageManager(CloudStorageProviderType providerType, string connectionData) =>
            CreateCloudStorageManager(default, providerType.ToString(), "test", connectionData, true);

        private ICloudStorageManager? CreateCloudStorageManager(int id, string provider, string name, string connectionData, bool enabled)
        {
            if (provider == CloudStorageProviderType.Azure.ToString())
            {
                var azConnectionData = CloudStorageConnectionDataDecoder.DecodeAzure(connectionData);
                if (azConnectionData is null)
                {
                    _logger.LogWarning("Could not decode Azure connection data for cloud storage provider with id {0}", id);
                    return null;
                }

                var stAccountName = string.IsNullOrEmpty(name) ? azConnectionData.AccountName : name;

                ICloudStorageManager storageManager = enabled ?
                    new AzureCloudStorageManager(azConnectionData.AccountName, azConnectionData.AccountKey, stAccountName) :
                    new DisabledCloudStorageManager(CloudStorageProviderType.Azure, stAccountName);

                return storageManager;
            }
            else if (provider == CloudStorageProviderType.AWS.ToString())
            {
                var awsConnectionData = CloudStorageConnectionDataDecoder.DecodeAWS(connectionData);
                if (awsConnectionData is null)
                {
                    _logger.LogWarning("Could not decode AWS connection data for cloud storage provider with id {0}", id);
                    return null;
                }

                var stAccountName = string.IsNullOrEmpty(name) ? awsConnectionData.BucketName : name;

                ICloudStorageManager storageManager = enabled ?
                    new AWSCloudStorageManager(stAccountName, awsConnectionData.AccessKeyId, awsConnectionData.AccessKeySecret,
                        awsConnectionData.Region, awsConnectionData.BucketName) :
                    new DisabledCloudStorageManager(CloudStorageProviderType.AWS, stAccountName);

                return storageManager;
            }
            else if (provider == CloudStorageProviderType.GCP.ToString())
            {
                var gcpConnectionData = CloudStorageConnectionDataDecoder.DecodeGCP(connectionData);
                if (gcpConnectionData is null || !gcpConnectionData.IsValid)
                {
                    _logger.LogWarning("Could not decode GCP connection data for cloud storage provider with id {0}", id);
                    return null;
                }

                var stAccountName = string.IsNullOrEmpty(name) ? gcpConnectionData.ProjectId : name;

                ICloudStorageManager storageManager = enabled ?
                    new GoogleCloudStorageManager(_logFactory, gcpConnectionData.ProjectId, gcpConnectionData.ConnectionData, stAccountName) :
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
