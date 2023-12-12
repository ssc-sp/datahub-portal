namespace Datahub.Infrastructure.Services.Storage
{
    public static class CloudStorageConnectionDataDecoder
    {
        public static AzureConnectionData DecodeAzure(string connectionData)
        {
            var variables = (connectionData ?? string.Empty).Split('|');
            if (variables.Length == 2)
            {
                var azureConnectionData = new AzureConnectionData()
                {
                    AccountName = variables[0],
                    AccountKey = variables[1]
                };
                return azureConnectionData;
            }
            else
            {
                return null;
            }
        }

        public static AWSConnectionData DecodeAWS(string connectionData)
        {
            var variables = (connectionData ?? string.Empty).Split('|');
            if (variables.Length == 4)
            {
                var awsConnectionData = new AWSConnectionData()
                {
                    AccessKeyId = variables[0],
                    AccessKeySecret = variables[1],
                    Region = variables[2],
                    BucketName = variables[3]
                };
                return awsConnectionData;
            }
            else
            {
                return null;
            }
        }

        public static GCPConnectionData DecodeGCP(string connectionData)
        {
            if (string.IsNullOrEmpty(connectionData))
            {
                return null;
            }
            else
            {
                return new GCPConnectionData() { ConnectionData = connectionData };
            }
        }
    }
}
