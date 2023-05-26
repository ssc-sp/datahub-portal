using System;

namespace Datahub.Core.Data.Databricks;

public class WorkspaceListEntry
{
    public ListEntryType EntryType { get; set; }
    public string Path { get; set; }
    public long ObjectId { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    public WorkspaceListEntry(string objectType, string path, long objectId, long? createdAt = null, long? modifiedAt = null)
    {
        EntryType = objectType switch
        {
            "DIRECTORY" => ListEntryType.Directory,
            "NOTEBOOK" => ListEntryType.Notebook,
            _ => throw new ArgumentException($"Unknown object type: {objectType}")
        };
        
        Path = path;
        ObjectId = objectId;
        CreatedAt = createdAt.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(createdAt.Value).UtcDateTime : null;
        ModifiedAt = modifiedAt.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(modifiedAt.Value).UtcDateTime : null;
    }

    public enum ListEntryType
    {
        Directory,
        Notebook
    }
}