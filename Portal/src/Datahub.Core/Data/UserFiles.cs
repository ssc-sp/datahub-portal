using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Azure.Search.Documents.Indexes;
using Microsoft.AspNetCore.Components.Forms;

namespace Datahub.Core.Data;

public class VersionMetadata
{
    public string Folderowner { get; set; }
    public string Folderid { get; set; }
    public string Createdby { get; set; }
    public string Lastmodifiedby { get; set; }
    public string Filename { get; set; }
    public string Fileformat { get; set; }
    public string Securityclass { get; set; }
    public string Ownedby { get; set; }
    public int Filesize { get; set; }
    public DateTime Uploadeddate { get; set; }
}

public class Version
{
    public string Versionid { get; set; }
    public VersionMetadata Metadata { get; set; }
    public string Timestamp { get; set; }
    public int Index { get; set; }
}

/// <summary>
/// The type of metadata (Folder needs to be less than File, for sorting)
/// </summary>
public enum MetadataType
{
    Folder = 1,
    File = 2
}

public class BaseMetadata : IEquatable<BaseMetadata>, IComparable<BaseMetadata>
{
    [JsonIgnore]
    public MetadataType DataType { get; set; }

    [JsonIgnore]
    public Folder Parent { get; set; }

    [JsonIgnore]
    public string Id { get; set; }

    [JsonIgnore]
    public string Name { get; set; }

    [JsonIgnore]
    public bool IsShared { get; set; }

    [SimpleField(IsFilterable = true)]
    public List<Activity> Activities { get; set; } = new List<Activity>();

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Createdby { get; set; }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public virtual string Ownedby { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTime Createdts { get; set; }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Lastmodifiedby { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTime Lastmodifiedts { get; set; }

    public int CompareTo(BaseMetadata other)
    {
        // A null value means that this object is greater.
        if (other == null)
        {
            return 1;
        }

        // Folder's go before Files
        int cmp = ((int)this.DataType).CompareTo((int)other.DataType);
        if (cmp == 0)
        {
            cmp = this.Name.CompareTo(other.Name);
        }

        return cmp;
    }

    public bool Equals(BaseMetadata other)
    {
        return this.CompareTo(other) == 0;
    }
}

public class Folder : BaseMetadata
{
    public Folder()
    {
        DataType = MetadataType.Folder;
    }

    public bool SortAscending { get; set; } = true;

    public override string Ownedby
    {
        get
        {
            return Createdby;
        }
        set
        {
        }
    }

    public List<BaseMetadata> Children { get; set; } = new List<BaseMetadata>();

    [JsonIgnore]
    public List<Folder> SubFolders
    {
        get
        {
            return Children.OfType<Folder>().ToList();
        }
    }

    [JsonIgnore]
    public List<FileMetaData> AllFiles
    {
        get
        {
            return Children.OfType<FileMetaData>().ToList();
        }
    }

    [JsonIgnore]
    public string FullPathFromRoot
    {
        get
        {
            if (Parent != null)
            {
                return $"{Parent.FullPathFromRoot}/{Id}";
            }

            return Id;
        }
    }

    [JsonIgnore]
    public string RootFolderName
    {
        get
        {
            if (Parent != null)
            {
                return Parent.RootFolderName;
            }

            return Id;
        }
    }

    public virtual void Add(BaseMetadata child, bool sort = true)
    {
        Children.Add(child);
        child.Parent = this;
        child.IsShared = this.IsShared;
        if (sort)
        {
            this.Sort();
        }
    }
    public virtual void Add(FileMetaData file, bool sort = true)
    {
        file.Folderpath = this.FullPathFromRoot;
        Add((BaseMetadata)file, sort);
    }

    public void Remove(BaseMetadata child, bool sort = true)
    {
        Children.Remove(child);
        if (sort)
        {
            this.Sort();
        }
    }

    public void Clear()
    {
        Children.Clear();
    }

    public void Sort()
    {
        Children.Sort((a, b) => SortAscending ? a.CompareTo(b) : b.CompareTo(a));
    }
}

/// <summary>
/// This is used by shared and search results.
/// We only contain a list of files.
/// No hierarchy.
/// Child has no reference to its parent
/// </summary>
public class NonHierarchicalFolder : Folder
{
    public NonHierarchicalFolder()
    {
        DataType = MetadataType.Folder;
    }

    [JsonIgnore]
    public new string FullPathFromRoot
    {
        get
        {
            return string.Empty;
        }
    }

    [JsonIgnore]
    public new string RootFolderName
    {
        get
        {
            return Id;
        }
    }

    public new void Add(FileMetaData file, bool sort = true)
    {
        Children.Add(file);
        file.IsShared = this.IsShared;
        if (sort)
        {
            this.Sort();
        }
    }
}

public class Customfield
{
    [SearchableField(IsFilterable = true)]
    public string Key { get; set; }
    [SearchableField(IsFilterable = true)]
    public string Value { get; set; }
}

public class Sharedwith
{
    [SearchableField(IsFilterable = true)]
    public string Userid { get; set; }
    [SearchableField(IsFilterable = true)]
    public string Role { get; set; }
}

public class Activity
{
    public string ActivityName { get; set; }
    public string Userid { get; set; }
    public DateTime Activityts { get; set; }
}

public class FileMetaData : BaseMetadata
{
    public const string FileId = "fileid";
    public const string OwnedBy = "ownedby";
    public const string CreatedBy = "createdby";
    public const string LastModifiedBy = "lastmodifiedby";
    public const string LastModified = "lastmodifiedts";
    public const string FileSize = "filesize";

    public FileMetaData()
    {
        DataType = MetadataType.File;
    }

    public DateTime Modified => Lastmodifiedts;

    [SimpleField(IsKey = true, IsFilterable = true)]
    public string Fileid
    {
        get
        {
            return Id;
        }
        set
        {
            Id = value;
        }
    }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Filename
    {
        get
        {
            return Name;
        }
        set
        {
            Name = value;
        }
    }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Folderpath { get; set; }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Fileformat
    {
        get
        {
            return !string.IsNullOrWhiteSpace(Filename) ? Path.GetExtension(Filename).TrimStart('.') : string.Empty;
        }
        set
        {
            // Do nothing as we will always use filename!!!!
        }
    }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    [Required(ErrorMessage = "The Security Classification field is required.")]
    public string Securityclass { get; set; }

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Description { get; set; }

    [SimpleField(IsFilterable = true)]
    public List<Customfield> Customfields { get; set; } = new List<Customfield>();

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Filesize { get; set; }

    [SimpleField(IsFilterable = true)]
    public List<Sharedwith> Sharedwith { get; set; } = new List<Sharedwith>();

    [SearchableField(IsFilterable = true, IsSortable = true)]
    public string Isdeleted { get; set; } = "false";

    [JsonIgnore]
    public string UploadStatus { get; set; } = FileUploadStatus.None;

    [JsonIgnore]
    public long UploadedBytes { get; set; }

    [JsonIgnore]
    public long BytesToUpload { get; set; }

    [JsonIgnore]
    public Stream FileData { get; set; }

    [JsonIgnore]
    public Dictionary<string, string> PermissionsDict { get; set; } = new Dictionary<string, string>();

    [JsonIgnore]
    public string CustomKey { get; set; }

    [JsonIgnore]
    public string CustomValue { get; set; }

    [JsonIgnore]
    public string Tags { get; set; }

    [JsonIgnore]
    public IBrowserFile BrowserFile { get; set; }

    [JsonIgnore]
    public string FullPathFromRoot
    {
        get
        {
            if (Parent != null)
            {
                return $"{Parent.FullPathFromRoot}/{Filename}";
            }

            return Filename;
        }
    }

    public void FinishUploadInfo(string status)
    {
        this.UploadedBytes = 0;
        this.BytesToUpload = 0;
        if (this.FileData != null)
        {
            this.FileData.Close();
            this.FileData.Dispose();
            this.FileData = null;
        }
        this.UploadStatus = status;
    }
}

public class ExpandableItem<T>
{
    public T Item { get; set; }
    public bool Expanded { get; set; }
    public bool Selected { get; set; }

    public int Level
    {
        get
        {
            if (Parent != null)
            {
                return Parent.Level + 1;
            }

            return 0;
        }
    }

    public bool HasChildren
    {
        get
        {
            return Children.Count > 0;
        }
    }

    public ExpandableItem<T> Parent { get; set; }
    public List<ExpandableItem<T>> Children { get; set; } = new List<ExpandableItem<T>>();

    public void Add(ExpandableItem<T> child)
    {
        Children.Add(child);
        child.Parent = this;
    }
}

/// <summary>
/// Keep this clAss As it is used by retrieval api
/// </summary>
public class UserFiles
{
    public List<Folder> Folders { get; set; } = new List<Folder>();
    public List<FileMetaData> Files { get; set; } = new List<FileMetaData>();
}