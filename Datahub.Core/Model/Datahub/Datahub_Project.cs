using Elemental.Components;
using Datahub.Shared.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datahub.Shared.EFCore
{

    public class Datahub_Project_Access_Request
    {
        [Key]
        public int Request_ID { get; set; }

        [Required]
        [StringLength(200)]
        public string User_Name { get; set; }

        [StringLength(200)]
        public string User_ID { get; set; }

        public bool Databricks { get; set; }
        public bool PowerBI { get; set; }
        public bool WebForms { get; set; }

        [NotMapped]
        [AeFormIgnore]
        public string RequestServiceType => (Databricks ? "Databricks" : (PowerBI ? "PowerBI" : "Web Forms"));

        public DateTime Request_DT { get; set; }

        public DateTime? Completion_DT { get; set; }

        public Datahub_Project Project { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }
    }

    public class Datahub_Project_User {

        [AeFormIgnore]
        [Key]

        public int ProjectUser_ID { get; set; }

        [StringLength(200)]
        public string User_ID { get; set; }
        public DateTime? Approved_DT { get; set; }

        public string ApprovedUser { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsDataApprover { get; set; }
        public Datahub_Project Project { get; set; }

        [StringLength(200)]
        public string User_Name {  get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

    }

    public class Datahub_Project_User_Request
    {

        [Key]
        public int ProjectUserRequest_ID { get; set; }
        
        [StringLength(200)]
        public string User_ID { get; set; }

        public DateTime Requested_DT { get; set; }

        public DateTime? Approved_DT { get; set;  }
        
        public string ApprovedUser { get; set; }
        
        public Datahub_Project Project { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

    }

    public class Datahub_Project_Pipeline_Lnk
    {
        public int Project_ID { get; set; }

        [ForeignKey("Project_ID")]
        public Datahub_Project Project { get; set; }
        public string Process_Nm { get; set; }

    }

    public class Datahub_Project: IComparable<Datahub_Project>
    {
        public const string ONGOING = "Ongoing";
        public const string CLOSED = "Closed";
        public const string ON_HOLD = "On Hold";

        public const string SQL_SERVER_DB_TYPE = "SQL Server";
        public const string POSTGRES_DB_TYPE = "Postgres";

        [AeFormIgnore]
        [Key]

        public int Project_ID { get; set; }

        [Required]
        [StringLength(10)]

        public string Sector_Name { get; set; }

        [StringLength(200)]
        public string Branch_Name { get; set; }

        public string Division_Name { get; set; }

        public string Contact_List { get; set; }

        [StringLength(100)]
        public string Project_Name { get; set; }

        [StringLength(100)]
        public string Project_Name_Fr { get; set; }

        [Required]
        [StringLength(10)]
        public string Project_Acronym_CD { get; set; }

        public string Project_Admin { get; set; }
        public string Project_Summary_Desc { get; set; }
        public string Project_Summary_Desc_Fr { get; set; }

        [AeLabel(isDropDown: true)] 
        public string Project_Category { get; set; }
        public DateTime Initial_Meeting_DT { get; set; }

        public int? Number_Of_Users_Involved { get; set; }
        public bool Is_Private { get; set; }

        
        public bool Is_Featured { get; set; }

        [Required]
        [AeLabel(validValues: new[] { "Unclassified", "Protected A", "Protected B" })]
        public string Data_Sensitivity { get; set; } = "Unclassified";

        public string Stage_Desc { get; set; }

        

        [Required]
        [AeLabel(validValues: new[] { ONGOING, CLOSED, ON_HOLD })]
        public string Project_Status_Desc { get; set; }

        [AeLabel(isDropDown: true)]
        public string Project_Phase { get; set; }

        public string GC_Docs_URL { get; set; }

        public string Project_Icon { get; set; }

        [AeFormIgnore]
        public string Comments_NT { get; set; }

        public DateTime? Last_Contact_DT { get; set; }

        public DateTime? Next_Meeting_DT { get; set; }

        public PBI_License_Request PBI_License_Request { get; set; }

        [AeFormIgnore]
        public DateTime Last_Updated_DT { get; set; }

        [AeFormIgnore]
        public DateTime? Deleted_DT { get; set; }

        public List<Datahub_ProjectComment> Comments { get; set; }

        public List<Datahub_Project_User> Users { get; set; }    
        
        public List<Datahub_Project_Access_Request> Requests { get; set; }

        [StringLength(400)]
        public string Databricks_URL { get; set; }

        [StringLength(400)]
        public string PowerBI_URL { get; set; }

        [StringLength(400)]
        public string WebForms_URL { get; set; }

        public List<WebForm> WebForms { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }

        [StringLength(128)]
        public string DB_Name { get; set; }

        [StringLength(128)]
        public string DB_Server { get; set; }

        [StringLength(100)]
        [AeLabel(validValues: new [] {SQL_SERVER_DB_TYPE, POSTGRES_DB_TYPE})]
        public string DB_Type { get; set; }

        public bool IsDatabasePostgres => DB_Type == POSTGRES_DB_TYPE;
        public bool IsDatabaseSqlServer => DB_Type == SQL_SERVER_DB_TYPE;
        public bool HasAssociatedDatabase => IsDatabasePostgres || IsDatabaseSqlServer;

        public List<Datahub_Project_Pipeline_Lnk> Pipelines { get; set; }

        public List<Project_Storage> StorageAccounts { get; set; }

        public List<Project_Database> Databases { get; set; }

        public List<Project_PBI_Report> PBI_Reports { get; set; }

        public List<Project_PBI_DataSet> PBI_DataSets { get; set; }

        public List<Project_PBI_Workspace> PBI_Workspaces { get; set; }

        [AeFormIgnore]
        [NotMapped]
        public string ProjectName
        {
            get
            {
                if (Thread.CurrentThread.CurrentCulture.Name.Equals("fr-ca", StringComparison.OrdinalIgnoreCase))
                {
                    return !string.IsNullOrWhiteSpace(Project_Name_Fr) ? Project_Name_Fr : Project_Name + " (*)";
                }
                return Project_Name;
            }
        }

        [AeFormIgnore]
        [NotMapped]
        public string ProjectDescription
        {
            get
            {
                if (Thread.CurrentThread.CurrentCulture.Name.Equals("fr-ca", StringComparison.OrdinalIgnoreCase))
                {
                    return !string.IsNullOrWhiteSpace(Project_Summary_Desc_Fr) ? Project_Summary_Desc_Fr : Project_Summary_Desc;
                }
                return Project_Summary_Desc;
            }
        }

        [AeFormIgnore]
        [NotMapped]
        public DatahubProjectInfo ProjectInfo
        {
            get
            {
                return new DatahubProjectInfo()
                {
                    ProjectNameEn = Project_Name,
                    ProjectNameFr = Project_Name_Fr
                };
            }
        }

        public int CompareTo(Datahub_Project other)
        {
            if (Project_Acronym_CD is null || other.Project_Acronym_CD is null)
                return Project_ID.CompareTo(other.Project_ID);
            return Project_Acronym_CD.CompareTo(other.Project_Acronym_CD);
        }
    }

    public class Datahub_ProjectComment
    {
        [Key]
        [AeFormIgnore]

        public int Comment_ID { get; set; }

        public DateTime Comment_Date_DT { get; set; }

        public string Comment_NT { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
        public Datahub_Project Project { get; set; }
    }

    public class Datahub_ProjectServiceRequests
    {
        // TODO add requesting user to data model

        [Key]
        [AeFormIgnore]

        public int ServiceRequests_ID { get; set; }

        public DateTime ServiceRequests_Date_DT { get; set; }

        public string ServiceType { get; set; }
        public DateTime? Is_Completed { get; set; }
        
        [StringLength(200)]
        public string User_Name { get; set; }

        [StringLength(200)]
        public string User_ID { get; set; }

        public DateTime? Notification_Sent { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
        public Datahub_Project Project { get; set; }
    }

    public class Datahub_Project_Costs
    {
        [Key]
        public int ProjectCosts_ID { get; set; }

        public int Project_ID { get; set; }

        [StringLength(10)]
        public string Project_Acronym_CD { get; set; }

        public DateTime Usage_DT { get; set; }

        public double Cost_AMT { get; set; }

        public DateTime Updated_DT { get; set; }
    }

    public class Datahub_Project_Sectors_And_Branches
    {
        [Key]
        public int SectorAndBranchS_ID { get; set; }

        public int Organization_ID { get; set; }

        [StringLength(4000)]
        public string Full_Acronym_E { get; set; }
        [StringLength(4000)]
        public string Full_Acronym_F { get; set; }
        [StringLength(4000)]
        public string Org_Acronym_E { get; set; }
        [StringLength(4000)]
        public string Org_Acronym_F { get; set; }
        [StringLength(4000)]
        public string Org_Name_E { get; set; }
        [StringLength(4000)]
        public string Org_Name_F { get; set; }
        [StringLength(1)]
        public string Org_Level { get; set; }
        public int? Superior_OrgId { get; set; }
    }
}
