namespace Datahub.Infrastructure.Services.Storage
{
    public class AWSConnectionData
    {
        public string AccessKeyId { get; set; }
        public string AccessKeySecret { get; set; }
        public string Region { get; set; }
        public string BucketName { get; set; }

        public string ConnectionData => $"{AccessKeyId}|{AccessKeySecret}|{Region}|{BucketName}";
    }
}
