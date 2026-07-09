namespace Datahub.Core.Data;

public class StorageMetadata
{
    public required string Container { get; set; }
}

public class AzureStorageMetadata : StorageMetadata
{
    public string? AccountName { get; set; }
    public required string Url { get; set; }
    public string? StorageAccountType { get; set; }
    public string? GeoRedundancy { get; set; }
    public required bool Versioning { get; set; }
}
