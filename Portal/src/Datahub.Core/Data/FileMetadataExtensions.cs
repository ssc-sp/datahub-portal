using System.Reflection;
using System.Text.RegularExpressions;
using Datahub.Shared.Entities;
using Newtonsoft.Json;

namespace Datahub.Core.Data;

public static class FileMetaDataExtensions
{
    private static readonly Type FileType = typeof(FileMetadata);
    private static readonly HashSet<string> JsonMetadataProperties =
    [
        FileMetadata.CustomFields,
        FileMetadata.SharedWith,
        FileMetadata.Tags,
        BaseMetadata.Activities
    ];

    public static void ParseDictionary(this FileMetadata fileMetadata, IDictionary<string, string> metadata)
    {
        if (metadata?.Count > 0)
        {
            foreach (string propertyName in fileMetadata.GetMetadataProperties().Where(p => !string.IsNullOrWhiteSpace(p.Key)).Select(p => p.Key))
            {
                if (metadata.ContainsKey(propertyName))
                {
                    PropertyInfo? info = FileType.GetProperty(propertyName);
                    if (info != null)
                    {
                        var value = metadata[propertyName];
                        if (!string.IsNullOrWhiteSpace(value))
                        {
                            if (info.PropertyType == typeof(DateTime))
                            {
                                // note: there is a chance the date is not valid, find out why?
                                if (DateTime.TryParse(value, out DateTime dt))
                                {
                                    info.SetValue(fileMetadata, dt);
                                }
                            }
                            else if (propertyName == FileMetadata.CustomFields)
                            {
                                info.SetValue(fileMetadata, JsonConvert.DeserializeObject<List<Customfield>>(value));
                            }
                            else if (propertyName == FileMetadata.SharedWith)
                            {
                                info.SetValue(fileMetadata, JsonConvert.DeserializeObject<List<Sharedwith>>(value));
                            }
                            else if (propertyName == FileMetadata.Tags)
                            {
                                info.SetValue(fileMetadata, JsonConvert.DeserializeObject<List<string>>(value));
                            }
                            else if (propertyName == BaseMetadata.Activities)
                            {
                                info.SetValue(fileMetadata, JsonConvert.DeserializeObject<List<Activity>>(value));
                            }
                            else
                            {
                                info.SetValue(fileMetadata, value);
                            }
                        }
                    }
                }
            }
        }
    }

    public static long FilesizeBytes(this FileMetadata file)
    {
        long bytes = 0;
        if (!long.TryParse(file.filesize, out bytes))
        {
            bytes = 0;
        }

        return bytes;
    }
    public static long TotalSpace(this Folder folder)
    {
        long total = folder.AllFiles.Sum(file => file.FilesizeBytes());

        total += folder.SubFolders.Sum(f => f.TotalSpace());
        return total;
    }

    public static string GetMetadataPropertyValue(this FileMetadata fileMetadata, string propertyName)
    {
        PropertyInfo? info = FileType.GetProperty(propertyName);
        if (info != null)
        {
            var value = info.GetValue(fileMetadata);
            if (value != null)
            {
                if (JsonMetadataProperties.Contains(propertyName))
                {
                    return JsonConvert.SerializeObject(value);
                }

                return value.ToString()!;
            }
        }

        return string.Empty;
    }

    public static Dictionary<string, string> GenerateMetadata(this FileMetadata fileMetadata)
    {
        Dictionary<string, string> metadata = new Dictionary<string, string>();
        FileMetaDataExtensions.GetMetadataProperties(null).Where(p => !string.IsNullOrWhiteSpace(p.Key)).Select(p => p.Key).ToList().ForEach(propertyName =>
        {
            metadata.Add(propertyName, fileMetadata.GetMetadataPropertyValue(propertyName));
        });

        return metadata;
    }

    public static List<(string Key, bool InSearch, bool IsVisible)> GetMetadataProperties(this FileMetadata? fileMetadata)
    {
        return new List<(string Key, bool InSearch, bool IsVisible)>
        {
            (BaseMetadata.Activities, true, false),
            (FileMetadata.FileId, true, false),
            (FileMetadata.Filename, true, true),
            (BaseMetadata.CreatedBy, true, false),
            (BaseMetadata.CreatedTs, true, false),
            (BaseMetadata.LastModifiedBy, true, false),
            (BaseMetadata.LastModifiedTs, true, true),
            (FileMetadata.SecurityClass, true, false),
            (BaseMetadata.OwnedBy, true, true),
            (FileMetadata.FileSize, true, true),
            (FileMetadata.FileFormat, true, true),
            (FileMetadata.FolderPath, true, true),
            (FileMetadata.SharedWith, false, false),
            (FileMetadata.Description, false, true),
            (FileMetadata.IsDeleted, true, false),
            (FileMetadata.Tags, false, true),
            (FileMetadata.CustomFields, false, true),
            (FileMetadata.UploadedDate, false, true)
        };
    }

    public static List<(string? Username, string Verb, string? Filename, string Location, string TimeSince)> GetActivity(this FileMetadata fileMetadata)
    {
        return new()
        {
            (fileMetadata.createdby, "created", fileMetadata.filename, "Root", fileMetadata.createdts.ToShortDateString())
        };
    }

    public static string GetFolderIdForJS(string id)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            return Regex.Replace(id, @"[^a-zA-Z0-9]", string.Empty);
        }

        return string.Empty;
    }
}
