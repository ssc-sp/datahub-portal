namespace Datahub.Core.Data;

public class APITargets
{
    public string SearchServiceName { get; set; } = null!;
    public string StorageAccountName { get; set; } = null!;
    public string KeyVaultName { get; set; } = null!;
    public string KeyVaultApiKeyPath { get; set; } = null!;
    public string FileSystemName { get; set; } = null!;
    public string CognitiveSearchURL { get; set; } = null!;
    public string LogoutURL { get; set; } = null!;
    public string LoginURL { get; set; } = null!;
    public string FileIndexName { get; set; } = null!;
    public string FileIndexerName { get; set; } = null!;
}
