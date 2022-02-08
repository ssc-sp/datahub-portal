using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using Datahub.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Datahub.Core.Data
{
    public static class FileMetaDataExtensions
    {
        public static void ParseDictionary(this FileMetaData fileMetadata, IDictionary<string, string> metadata)
        {
            if (metadata?.Count > 0)
            {
                foreach(string propertyName in fileMetadata.GetMetadataProperties().Where(p => !string.IsNullOrWhiteSpace(p.key)).Select( p => p.key))
                {
                    if (metadata.ContainsKey(propertyName))
                    {
                        PropertyInfo info = fileType.GetProperty(propertyName);
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
                                else if (propertyName == "customfields")
                                {
                                   info.SetValue(fileMetadata, JsonConvert.DeserializeObject<List<Customfield>>(value));
                                }
                                else if (propertyName == "sharedwith")
                                {
                                   info.SetValue(fileMetadata, JsonConvert.DeserializeObject<List<Sharedwith>>(value));
                                }
                                else if (propertyName == "tags")
                                {
                                   info.SetValue(fileMetadata, JsonConvert.DeserializeObject<List<string>>(value));
                                }
                                else if (propertyName == "activities")
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

        public static long FilesizeBytes(this FileMetaData file)
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

        
        private static Type fileType = typeof(FileMetaData);

        public static string GetMetadataPropertyValue(this FileMetaData fileMetadata, string propertyName)
        {
            PropertyInfo info = fileType.GetProperty(propertyName);
            if (info != null)
            {
                var value = info.GetValue(fileMetadata);
                if (value != null)
                {
                    if (propertyName == "customfields" || propertyName == "sharedwith" || propertyName == "tags" || propertyName == "activities")
                    {
                        return JsonConvert.SerializeObject(value);
                    }

                    return value.ToString();
                }
            }

            return string.Empty;
        }

        public static Dictionary<string, string> GenerateMetadata(this FileMetaData fileMetadata)
        {
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            FileMetaDataExtensions.GetMetadataProperties(null).Where(p => !string.IsNullOrWhiteSpace(p.key)).Select(p => p.key).ToList().ForEach(propertyName =>
            {
                metadata.Add(propertyName, fileMetadata.GetMetadataPropertyValue(propertyName));
            });

            return metadata;
        }
        
        public static List<(string key, bool inSearch, bool isVisible)> GetMetadataProperties(this FileMetaData fileMetadata)
        {
            return new List<(string key, bool inSearch, bool isVisible)>
            {
                ("activities", true, false),
                ("fileid", true, false),
                ("filename", true, true),
                ("createdby", true, false),
                ("createdts", true, false),
                ("lastmodifiedby", true, false),
                ("lastmodifiedts", true, true),
                ("securityclass", true, false),
                ("ownedby", true, true),
                ("filesize", true, true),
                ("fileformat", true, true),
                ("folderpath", true, true),
                ("sharedwith", false, false),
                ("description", false, true),
                ("isdeleted", true, false),
                ("tags", false, true),
                ("customfields", false, true),
                ("uploadeddate", false, true)
            };
        }

        public static List<(string username, string verb, string filename, string location, string timeSince)> GetActivity(this FileMetaData fileMetadata)
        {
            return new List<(string username, string verb, string filename, string location, string timeSince)>()
            {
                (fileMetadata.createdby, "created", fileMetadata.filename, "Root", fileMetadata.createdts.ToShortDateString())
            };
        }

        public static string GetFolderIdForJS(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                return Regex.Replace(id, @"[^a-zA-Z0-9]", "");
            }

            return string.Empty;
        }
    }
}
