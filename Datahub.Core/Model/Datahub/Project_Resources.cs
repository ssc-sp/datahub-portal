﻿using Newtonsoft.Json;
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

        [StringLength(200)]
        public string InputClassName { get; set; }

        public string InputJsonContent { get; set; }

        public T GetResourceObject<T>()
        {
            if (typeof(T).FullName != ClassName)
            {
                throw new InvalidCastException($"Resource {ResourceId} has type {ClassName} (tried getting {typeof(T).FullName})");
            }

            return string.IsNullOrEmpty(JsonContent) ? default : JsonConvert.DeserializeObject<T>(JsonContent);
        }

        public void SetResourceObject<T>(T obj)
        {
            ClassName = typeof(T).FullName;
            if (obj != null)
            {
                JsonContent = JsonConvert.SerializeObject(obj);
            }
        }

        public T GetInputParameters<T>() where T : IProjectResourceInput
        {
            if (typeof(T).FullName != InputClassName)
            {
                throw new InvalidCastException($"Resource {ResourceId} has input parameter type {InputClassName} (tried getting {typeof(T).FullName})");
            }

            return string.IsNullOrEmpty(InputJsonContent) ? default : JsonConvert.DeserializeObject<T>(InputJsonContent);
        }

        public void SetInputParameters<T>(T obj) where T: IProjectResourceInput
        {
            var realType = obj.GetType();
            InputClassName = realType.FullName;
            if (obj != null)
            {
                InputJsonContent = JsonConvert.SerializeObject(obj);
            }
        }
    }

    public static class ProjectResourceConstants
    {
        public const string SERVICE_TYPE_POSTGRES = "psql";
        public const string SERVICE_TYPE_SQL_SERVER = "sql";
        public const string SERVICE_TYPE_STORAGE = "storage";
        public const string SERVICE_TYPE_DATABRICKS = "databricks";
        public const string SERVICE_TYPE_POWERBI = "powerbi";

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

    public interface IProjectResourceInput { }

    public class ProjectResourceInput_Storage: IProjectResourceInput
    {
        public string Storage_Type { get; set; }
    }
}
