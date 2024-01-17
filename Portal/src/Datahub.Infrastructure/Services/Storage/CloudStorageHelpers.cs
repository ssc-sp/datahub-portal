namespace Datahub.Infrastructure.Services.Storage
{
    public static class CloudStorageHelpers
    {
        public const string AZ_AccountName = "AccountName";
        public const string AZ_AccountKey = "AccountKey";

        public const string AWS_AccesKeyId = "AccessKeyId";
        public const string AWS_AccessKeySecret = "AccessKeySecret";
        public const string AWS_Region = "Region";
        public const string AWS_BucketName = "BucketName";

        public const string GCP_ProjectId = "GCPProjectId";
        public const string GCP_Json = "GCPJson";

        public readonly static string[] All_Keys = { AZ_AccountName, AZ_AccountKey, AWS_AccesKeyId, AWS_AccessKeySecret, AWS_Region, AWS_BucketName, GCP_Json };
    }
}
