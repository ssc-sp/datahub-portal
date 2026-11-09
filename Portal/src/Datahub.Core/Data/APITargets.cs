namespace Datahub.Core.Data;

public class APITargets
{
    public string SearchServiceName { get; set; } = string.Empty;
    public string StorageAccountName { get; set; } = string.Empty;
    public string KeyVaultName { get; set; } = string.Empty;
    public string KeyVaultApiKeyPath { get; set; } = string.Empty;
    public string FileSystemName { get; set; } = string.Empty;
    public string CognitiveSearchURL { get; set; } = string.Empty;
    public string LogoutURL { get; set; } = string.Empty;
    public string LoginURL { get; set; } = string.Empty;
    public string FileIndexName { get; set; } = string.Empty;
    public string FileIndexerName { get; set; } = string.Empty;
}
