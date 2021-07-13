using Elemental.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NRCan.Datahub.Data.Projects
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

        [Required]
        [StringLength(200)]
        public string User_Name { get; set; }

        [StringLength(200)]
        public string User_ID { get; set; }

        public bool? Databricks { get; set; }
        public bool? PowerBI { get; set; }
        public bool? WebForms { get; set; }

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

    public class Datahub_Project
    {

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

        public string Stage_Desc { get; set; }

        [Required]
        [AeLabel(validValues: new[] { "Ongoing", "Closed", "On Hold" })]
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

        public List<Datahub_Project_Pipeline_Lnk> Pipelines { get; set; }

        [AeFormIgnore]
        [NotMapped]
        public string ProjectName
        {
            get
            {
                if (Thread.CurrentThread.CurrentCulture.Name.Equals("fr-ca", StringComparison.OrdinalIgnoreCase))
                {
                    return !string.IsNullOrWhiteSpace(Project_Name_Fr) ? Project_Name_Fr : Project_Name;
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
        [Key]
        [AeFormIgnore]

        public int ServiceRequests_ID { get; set; }

        public DateTime ServiceRequests_Date_DT { get; set; }

        public string ServiceType { get; set; }
        public DateTime? Is_Completed { get; set; }

        [AeFormIgnore]
        [Timestamp]
        public byte[] Timestamp { get; set; }
        public Datahub_Project Project { get; set; }
    }
}
