namespace Datahub.Infrastructure.Services.Storage
{
    public static class CloudStorageHelpers
    {
        public const string AZAccountName = "AccountName";
        public const string AZAccountKey = "AccountKey";

        public const string AWSAccesKeyId = "AccessKeyId";
        public const string AWSAccessKeySecret = "AccessKeySecret";
        public const string AWSRegion = "Region";
        public const string AWSBucketName = "BucketName";

        public const string GCPProjectId = "GCPProjectId";
        public const string GCPJson = "GCPJson";

        public readonly static string[] AllKeys = { AZAccountName, AZAccountKey, AWSAccesKeyId, AWSAccessKeySecret, AWSRegion, AWSBucketName, GCPJson };
    }
}
