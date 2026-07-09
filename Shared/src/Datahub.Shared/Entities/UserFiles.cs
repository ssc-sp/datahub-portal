using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Datahub.Shared.Entities;

#pragma warning disable SA1300 // Element should begin with upper-case letter

public class VersionMetadata
{
    public required string folderowner { get; set; }
    public required string folderid { get; set; }
    public required string createdby { get; set; }
    public required string lastmodifiedby { get; set; }
    public required string filename { get; set; }
    public required string fileformat { get; set; }
    public required string securityclass { get; set; }
    public required string ownedby { get; set; }
    public int filesize { get; set; }
    public DateTime uploadeddate { get; set; }
}

public class FileVersion
{
    public required string versionid { get; set; }
    public required VersionMetadata metadata { get; set; }
    public required string timestamp { get; set; }
    public int index { get; set; }
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
    public const string Activities = "activities";
    public const string CreatedBy = "createdby";
    public const string OwnedBy = "ownedby";
    public const string CreatedTs = "createdts";
    public const string LastModifiedBy = "lastmodifiedby";
    public const string LastModifiedTs = "lastmodifiedts";

    [JsonIgnore]
    public MetadataType dataType { get; set; }

    [JsonIgnore]
    public Folder parent { get; set; } = null!;

    [JsonIgnore]
    public required string id { get; set; }

    [JsonIgnore]
    public string? name { get; set; }

    [JsonIgnore]
    public bool isShared { get; set; }

    public List<Activity> activities { get; set; } = new List<Activity>();

    public string? createdby { get; set; }

    public virtual string? ownedby { get; set; }

    public DateTime createdts { get; set; }

    public string? lastmodifiedby { get; set; }

    public DateTime lastmodifiedts { get; set; }

    public int CompareTo(BaseMetadata? other)
    {
        // A null value means that this object is greater.
        if (other == null)
        {
            return 1;
        }

        // Folder's go before Files
        int cmp = ((int)this.dataType).CompareTo((int)other.dataType);
        if (cmp == 0 && this.name is not null)
        {
            cmp = this.name.CompareTo(other.name);
        }

        return cmp;
    }

    public bool Equals(BaseMetadata? other)
    {
        return this.CompareTo(other) == 0;
    }
}

public class Folder : BaseMetadata
{
    public Folder()
    {
        dataType = MetadataType.Folder;
    }

    public bool sortAscending { get; set; } = true;

    public override string? ownedby
    {
        get
        {
            return this.createdby;
        }
        set
        {
        }
    }

    public List<BaseMetadata> children { get; set; } = new List<BaseMetadata>();

    [JsonIgnore]
    public List<Folder> SubFolders
    {
        get
        {
            return children.OfType<Folder>().ToList();
        }
    }

    [JsonIgnore]
    public List<FileMetadata> AllFiles
    {
        get
        {
            return children.OfType<FileMetadata>().ToList();
        }
    }

    [JsonIgnore]
    public string fullPathFromRoot
    {
        get
        {
            if (parent != null)
            {
                return $"{parent.fullPathFromRoot}/{id}";
            }

            return id;
        }
    }

    [JsonIgnore]
    public string rootFolderName
    {
        get
        {
            if (parent != null)
            {
                return parent.rootFolderName;
            }

            return id;
        }
    }

    public virtual void Add(BaseMetadata child, bool sort = true)
    {
        children.Add(child);
        child.parent = this;
        child.isShared = this.isShared;
        if (sort)
        {
            this.Sort();
        }
    }
    public virtual void Add(FileMetadata file, bool sort = true)
    {
        file.folderpath = this.fullPathFromRoot;
        Add((BaseMetadata)file, sort);
    }

    public void Remove(BaseMetadata child, bool sort = true)
    {
        children.Remove(child);
        if (sort)
        {
            this.Sort();
        }
    }

    public void Clear()
    {
        children.Clear();
    }

    public void Sort()
    {
        children.Sort((a, b) => sortAscending ? a.CompareTo(b) : b.CompareTo(a));
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
        dataType = MetadataType.Folder;
    }

    [JsonIgnore]
    public new string fullPathFromRoot
    {
        get
        {
            return string.Empty;
        }
    }

    [JsonIgnore]
    public new string rootFolderName
    {
        get
        {
            return id;
        }
    }

    public new void Add(FileMetadata file, bool sort = true)
    {
        children.Add(file);
        file.isShared = this.isShared;
        if (sort)
        {
            this.Sort();
        }
    }
}

public class Customfield
{
    public required string key { get; set; }
    public required string value { get; set; }
}

public class Sharedwith
{
    public required string userid { get; set; }
    public required string role { get; set; }
}

public class Activity
{
    public required string activity { get; set; }
    public required string userid { get; set; }
    public DateTime activityts { get; set; }
}

public class FileMetadata : BaseMetadata
{
    public const string FileId = "fileid";
    public const string Filename = "filename";
    public const string FolderPath = "folderpath";
    public const string FileFormat = "fileformat";
    public const string SecurityClass = "securityclass";
    public const string Description = "description";
    public const string Tags = "tags";
    public const string CustomFields = "customfields";
    public const string FileSize = "filesize";
    public const string SharedWith = "sharedwith";
    public const string IsDeleted = "isdeleted";
    public const string UploadedDate = "uploadeddate";
    public const string UploadBatchId = "uploadBatchId";
    public const string AvScan = "avscan";

    public FileMetadata()
    {
        dataType = MetadataType.File;
        folderpath = string.Empty;
        securityclass = string.Empty;
        description = string.Empty;
        filesize = string.Empty;
        fileData = Stream.Null;
        _customKey = string.Empty;
        _customValue = string.Empty;
        _tags = string.Empty;
    }

    public DateTime Modified => lastmodifiedts;

    public string fileid
    {
        get
        {
            return this.id;
        }
        set
        {
            this.id = value;
        }
    }

    public string? filename
    {
        get
        {
            return this.name;
        }
        set
        {
            this.name = value;
        }
    }

    public string folderpath { get; set; }

    public string fileformat
    {
        get
        {
            return !string.IsNullOrWhiteSpace(filename) ? Path.GetExtension(filename).TrimStart('.') : string.Empty;
        }
        set
        {
            // Do nothing as we will always use filename!!!!
        }
    }

    [Required(ErrorMessage = "The Security Classification field is required.")]
    public string securityclass { get; set; }

    public string description { get; set; }

    public List<string> tags
    {
        get
        {
            List<string> list = string.IsNullOrWhiteSpace(this._tags) ? new List<string>() : this._tags.Split(",").ToList();

            return list;
        }
        set
        {
            if (value == null || value.Count == 0)
            {
                this._tags = string.Empty;
            }
            else
            {
                this._tags = string.Join(",", value);
            }
        }
    }

    public List<Customfield> customfields { get; set; } = new List<Customfield>();

    public string filesize { get; set; }

    public List<Sharedwith> sharedwith { get; set; } = new List<Sharedwith>();

    public string isdeleted { get; set; } = "false";

    public string? uploadBatchId { get; set; }

    [JsonIgnore]
    public string uploadStatus { get; set; } = FileUploadStatus.None;

    [JsonIgnore]
    public long uploadedBytes { get; set; }

    [JsonIgnore]
    public long bytesToUpload { get; set; }

    [JsonIgnore]
    public Stream? fileData { get; set; }

    [JsonIgnore]
    public Dictionary<string, string> permissionsDict { get; set; } = new Dictionary<string, string>();

    [JsonIgnore]
    public string _customKey { get; set; }

    [JsonIgnore]
    public string _customValue { get; set; }

    [JsonIgnore]
    public string _tags { get; set; }

    [JsonIgnore]
    public string? fullPathFromRoot
    {
        get
        {
            if (parent != null)
            {
                return $"{parent.fullPathFromRoot}/{filename}";
            }

            return filename;
        }
    }

    public void FinishUploadInfo(string status)
    {
        this.uploadedBytes = 0;
        this.bytesToUpload = 0;
        if (this.fileData != null)
        {
            this.fileData.Close();
            this.fileData.Dispose();
            this.fileData = null;
        }
        this.uploadStatus = status;
    }
}

public class ExpandableItem<T>
{
    public required T item { get; set; }
    public bool expanded { get; set; }
    public bool selected { get; set; }

    public int level
    {
        get
        {
            if (parent != null)
            {
                return parent.level + 1;
            }

            return 0;
        }
    }

    public bool hasChildren
    {
        get
        {
            return children.Count > 0;
        }
    }

    public required ExpandableItem<T> parent { get; set; }
    public List<ExpandableItem<T>> children { get; set; } = new List<ExpandableItem<T>>();

    public void Add(ExpandableItem<T> child)
    {
        children.Add(child);
        child.parent = this;
    }
}

/// <summary>
/// Keep this clAss As it is used by retrieval api
/// </summary>
public class UserFiles
{
    public List<Folder> folders { get; set; } = new List<Folder>();
    public List<FileMetadata> files { get; set; } = new List<FileMetadata>();
}

public static class FileUploadStatus
{
    public const string None = "None";
    public const string SelectedToUpload = "SelectedToUpload";
    public const string UploadedToBrowser = "UploadedToBrowser";
    public const string UploadingToRepository = "UploadingToRepository";
    public const string FileUploadSuccess = "FileUploadSuccess";
    public const string FileUploadError = "FileUploadError";
    public const string FileUploadCanceled = "FileUploadCanceled";
}

#pragma warning restore SA1300 // Element should begin with upper-case letter
