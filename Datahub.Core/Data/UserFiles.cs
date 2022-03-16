using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Forms;
using Tewr.Blazor.FileReader;

namespace Datahub.Core.Data
{

    public class VersionMetadata
    {
        public string folderowner { get; set; }
        public string folderid { get; set; }
        public string createdby { get; set; }
        public string lastmodifiedby { get; set; }
        public string filename { get; set; }
        public string fileformat { get; set; }
        public string securityclass { get; set; }
        public string ownedby { get; set; }
        public int filesize { get; set; }
        public DateTime uploadeddate { get; set; }
    }

    public class Version
    {
        public string versionid { get; set; }
        public VersionMetadata metadata { get; set; }
        public string timestamp { get; set; }
        public int index { get; set; }
    }

    /// <summary>
    /// The type of metadata (Folder needs to be less than File, for sorting)
    /// </summary>
    public enum MetadataType {
        Folder = 1,
        File = 2        
    }

    public class BaseMetadata : IEquatable<BaseMetadata> , IComparable<BaseMetadata>
    {
        [JsonIgnore]
        public MetadataType dataType { get; set; }

        [JsonIgnore]
        public Folder parent { get; set; }

        [JsonIgnore]
        public string id { get; set; }

        [JsonIgnore]
        public string name { get; set; }

        [JsonIgnore]
        public bool isShared { get; set; }

        [SimpleField(IsFilterable = true)]
        public List<Activity> activities { get; set; } = new List<Activity>();

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string createdby { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public virtual string ownedby { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true)]
        public DateTime createdts { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string lastmodifiedby { get; set; }

        [SimpleField(IsFilterable = true, IsSortable = true)]
        public DateTime lastmodifiedts { get; set; }

        public int CompareTo(BaseMetadata other)
        {
            // A null value means that this object is greater.
            if (other == null)
            {
                return 1;
            }

            // Folder's go before Files
            int cmp = ((int)this.dataType).CompareTo((int)other.dataType);
            if (cmp == 0)
            {
                cmp = this.name.CompareTo(other.name);
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
            dataType = MetadataType.Folder;
        }

        public bool sortAscending {get; set;} = true;

        public override string ownedby
        {
            get
            {
                return base.createdby;
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
        public List<FileMetaData> AllFiles
        {
            get
            {
                return children.OfType<FileMetaData>().ToList();
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
        public virtual void Add(FileMetaData file, bool sort = true)
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
            children.Sort((a, b) => (sortAscending) ? a.CompareTo(b) : b.CompareTo(a));
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
                return "";
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

        public new void Add(FileMetaData file, bool sort = true)
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
        [SearchableField(IsFilterable = true)]
        public string key { get; set; }
        [SearchableField(IsFilterable = true)]
        public string value { get; set; }
    }

    public class Sharedwith
    {
        [SearchableField(IsFilterable = true)]
        public string userid { get; set; }
        [SearchableField(IsFilterable = true)]
        public string role { get; set; }
    }

    public class Activity
    {
        public string activity { get; set; }
        public string userid { get; set; }
        public DateTime activityts { get; set; }
    }

    public class FileMetaData: BaseMetadata
    {
        public const string OwnedBy = "ownedby";
        public const string CreatedBy = "createdby";
        public const string LastModifiedBy = "lastmodifiedby";
        

        public FileMetaData()
        {
            dataType = MetadataType.File;
        }

        public DateTime Modified => lastmodifiedts;

        [SimpleField(IsKey = true, IsFilterable = true)]
        public string fileid
        {
            get
            {
                return base.id;
            }
            set
            {
                base.id = value;
            }
        }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string filename
        {
            get
            {
                return base.name;
            }
            set
            {
                base.name = value;
            }
        }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string folderpath { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
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

        [SearchableField(IsFilterable = true, IsSortable = true)]
        [Required(ErrorMessage = "The Security Classification field is required.")]
        public string securityclass { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string description { get; set; }

        [SimpleField(IsFilterable = true)]
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

        [SimpleField(IsFilterable = true)]
        public List<Customfield> customfields { get; set; } = new List<Customfield>();

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string filesize { get; set; }

        [SimpleField(IsFilterable = true)]
        public List<Sharedwith> sharedwith { get; set; } = new List<Sharedwith>();

        [SearchableField(IsFilterable = true, IsSortable = true)]
        public string isdeleted { get; set; } = "false";

        [JsonIgnore]
        public string uploadStatus { get; set; } = FileUploadStatus.None;

        [JsonIgnore]
        public long uploadedBytes { get; set; }

        [JsonIgnore]
        public long bytesToUpload { get; set; }

        [JsonIgnore]
        public Stream fileData { get; set; } 

        [JsonIgnore]
        public Dictionary<string, string> permissionsDict { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public string _customKey { get; set; }

        [JsonIgnore]
        public string _customValue { get; set; }

        [JsonIgnore]
        public string _tags { get; set; }
        
        [JsonIgnore]
        public IBrowserFile BrowserFile { get; set; }
      
        [JsonIgnore]
        public string fullPathFromRoot
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
        public T item { get; set; }
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

        public ExpandableItem<T> parent { get; set; }
        public List<ExpandableItem<T>> children  { get; set; } = new List<ExpandableItem<T>>();

        public void Add(ExpandableItem<T> child)
        {
            children.Add(child);
            child.parent = this;
        }
    }
    
    /// <summary>
    /// Keep this class as it is used by retrieval api
    /// </summary>
    public class UserFiles
    {
        public List<Folder> folders { get; set; } = new List<Folder>();
        public List<FileMetaData> files { get; set; } = new List<FileMetaData>();
    }


}
