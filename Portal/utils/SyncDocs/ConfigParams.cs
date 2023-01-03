namespace SyncDocs;
internal class ConfigParams
{
    public string Target { get; set; } = "";
    public string Excluded { get; set; } = "";

    public IEnumerable<string> GetExcludedFolders()
    {
        return (Excluded ?? "").Split(",", StringSplitOptions.RemoveEmptyEntries).Where(s => !string.IsNullOrEmpty(s));
    }        
}
