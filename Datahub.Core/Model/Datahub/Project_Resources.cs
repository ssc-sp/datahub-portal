using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.EFCore
{
    public class Project_Resources2
    {
        [Key]
        public Guid ResourceId { get; set; }

        [Required]
        [StringLength(200)]
        public string ResourceType { get; set; }

        [Required]
        [StringLength(200)]
        public string ClassName { get; set; }

        public string JsonContent { get; set; }

        [ForeignKey("Project")]
        public int ProjectId { get; set; }

        public DateTime TimeRequested { get; set; }

        public DateTime? TimeCreated { get; set; }

        public Datahub_Project Project { get; set; }

        public string InputJsonContent { get; set; }

        public bool HasInputParams => !string.IsNullOrEmpty(InputJsonContent);

        public T GetResourceObject<T>()
        {
            if (typeof(T).FullName != ClassName)
            {
                throw new InvalidCastException($"Resource {ResourceId} has type {ClassName} (tried getting {typeof(T).FullName})");
            }

            return string.IsNullOrEmpty(JsonContent) ? default : JsonConvert.DeserializeObject<T>(JsonContent);
        }

        public bool IsValid<T>()
        {
            if (typeof(T).FullName != ClassName)
            {
                return false;
            }
            if (string.IsNullOrEmpty(JsonContent))
                return false;
            try
            {
                JsonConvert.DeserializeObject<T>(JsonContent);
                return true;
            } catch (Exception)
            {
                return false;
            }
        }

        public void SetResourceObject<T>(T obj)
        {
            ClassName = typeof(T).FullName;
            if (obj != null)
            {
                JsonContent = JsonConvert.SerializeObject(obj);
            }
        }

        public class CaseInsensitiveSettingsDict : Dictionary<string, string>
        {
            public CaseInsensitiveSettingsDict() : base(StringComparer.OrdinalIgnoreCase) { }
        }

        public Dictionary<string, string> GetInputParamsDictionary()
        {
            return string.IsNullOrEmpty(InputJsonContent) ? default : JsonConvert.DeserializeObject<CaseInsensitiveSettingsDict>(InputJsonContent);
        }

        public void SetInputParameters(Dictionary<string, string> inputParams)
        {
            InputJsonContent = JsonConvert.SerializeObject(inputParams);
        }
    }

    

    public static class ProjectResourceConstants
    {
        public const string SERVICE_TYPE_POSTGRES = "psql";
        public const string SERVICE_TYPE_SQL_SERVER = "sql";
        public const string SERVICE_TYPE_STORAGE = "storage";
        public const string SERVICE_TYPE_DATABRICKS = "databricks";
        public const string SERVICE_TYPE_POWERBI = "powerbi";

        public static readonly string[] ALL_RESOURCE_TYPES = new[] 
        { 
            SERVICE_TYPE_DATABRICKS, 
            SERVICE_TYPE_SQL_SERVER, 
            SERVICE_TYPE_POSTGRES, 
            SERVICE_TYPE_POWERBI, 
            SERVICE_TYPE_STORAGE 
        };

        public const string STORAGE_TYPE_BLOB = "blob";
        public const string STORAGE_TYPE_GEN2 = "gen2";
    }

    public class ProjectResource_Database
    {
        public string Database_Type { get; set; }
        public string Database_Name { get; set; }
        public string Database_Server { get; set; }

        public bool IsPostgres => Database_Type == ProjectResourceConstants.SERVICE_TYPE_POSTGRES;
        public bool IsSqlServer => Database_Type == ProjectResourceConstants.SERVICE_TYPE_SQL_SERVER;
    }

    public class ProjectResource_Storage
    {
        public string Storage_Type { get; set; }
        public string Storage_Account { get; set; }
        public List<string> Containers { get; set; }

        public bool IsBlobStorage => Storage_Type == ProjectResourceConstants.STORAGE_TYPE_BLOB;
        public bool IsGen2Storage => Storage_Type == ProjectResourceConstants.STORAGE_TYPE_GEN2;
    }

    public class ProjectResource_Blank { }
}
