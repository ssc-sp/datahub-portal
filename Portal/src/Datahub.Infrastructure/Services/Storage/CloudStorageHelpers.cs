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

        public static string GetStorageClassLabel(string tier) => tier switch
        {
            "STANDARD" => "Standard",
            "STANDARD_IA" => "Standard - Infrequent Access",
            "ONEZONE_IA" => "One Zone - Infrequent Access",
            "INTELLIGENT_TIERING" => "Intelligent-Tiering",
            "GLACIER_IR" => "Glacier Instant Retrieval",
            "GLACIER" => "Glacier Flexible Retrieval",
            "DEEP_ARCHIVE" => "Glacier Deep Archive",
            "REDUCED_REDUNDANCY" => "Reduced Redundancy",
            "NEARLINE" => "Nearline",
            "COLDLINE" => "Coldline",
            "ARCHIVE" => "Archive",
            _ => tier
        };
    }
}
