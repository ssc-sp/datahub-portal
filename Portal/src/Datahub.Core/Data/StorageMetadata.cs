namespace Datahub.Core.Data;

public class StorageMetadata
{
    public required string AccountName { get; set; }
    public required string Url { get; set; }
    public required string StorageAccountType { get; set; }
    public required string GeoRedundancy { get; set; }
    public required string Versioning { get; set; }
    public required string Container { get; set; }
}