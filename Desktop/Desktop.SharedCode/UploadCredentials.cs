namespace Datahub.Core.DataTransfers
{
    public record UploadCredentials
    {
        public string WorkspaceCode { get; set; }
        public string SASToken { get; set; }
        public DateTimeOffset SASTokenExpiry { get; set; }
        public string DataHubEnvironment { get; set; }
    }
}
