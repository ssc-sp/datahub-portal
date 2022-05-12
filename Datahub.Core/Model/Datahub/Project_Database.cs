using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Core.EFCore
{
    //[Index(nameof(ResourceID), IsUnique = true)]
    //public class Project_Database
    //{
    //    public const string DB_TYPE_POSTGRES = "psql";
    //    public const string DB_TYPE_SQL_SERVER = "sql";

    //    [Key]
    //    public Guid Id { get; set; }

    //    [ForeignKey("Project")]
    //    public int ProjectID { get; set; }

    //    [ForeignKey("Resource")]
    //    public int ResourceID { get; set; }

    //    [Required]
    //    [StringLength(128)]
    //    public string Database_Name { get; set; }

    //    [Required]
    //    [StringLength(128)]
    //    public string Database_Server { get; set; }

    //    [Required]
    //    [StringLength(32)]
    //    public string Database_Type { get; set; }

    //    public bool IsSqlServerDb => Database_Type == DB_TYPE_SQL_SERVER;
    //    public bool IsPostgresDb => Database_Type == DB_TYPE_POSTGRES;

    //    public Datahub_Project Project { get; set; }
    //    public Project_Resources Resource { get; set; }
    //}

    public class Project_Resources
    {
        [Key]
        public int Id { get; set; }

        [StringLength(200)]
        public string ResourceType {  get; set; }
        [StringLength(200)]
        public string ResourceName {  get; set; }
        [StringLength(200)]
        public string Attributes {  get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Param3 { get; set; }
        public DateTime TimeRequested {  get; set; }
        public DateTime? TimeCreated {  get; set; }

        public Datahub_Project Project { get; set; }
    }
}