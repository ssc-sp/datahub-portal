﻿using Elemental.Components;
using Datahub.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using MudBlazor.Forms;
using AeFormCategoryAttribute = MudBlazor.Forms.AeFormCategoryAttribute;
using AeFormIgnoreAttribute = MudBlazor.Forms.AeFormIgnoreAttribute;
using System.Linq;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Notification;
using Datahub.Shared.Entities;

namespace Datahub.Core.Model.Datahub;

public enum ProjectStatus
{
    OnHold, InProgress, Support, Closed
}
public class Datahub_Project : IComparable<Datahub_Project>
{
    public const string ONGOING = "Ongoing";
    public const string CLOSED = "Closed";
    public const string ON_HOLD = "On Hold";

    public const string SQL_SERVER_DB_TYPE = "SQL Server";
    public const string POSTGRES_DB_TYPE = "Postgres";

    [AeFormIgnore]
    [Key]

    public int Project_ID { get; set; }


    [AeFormIgnore]
    public int? SectorId { get; set; }

    [AeFormCategory("Sector Information")]
    [MudForm(IsDropDown = true)]
    [ForeignKey("SectorId")]
    public Organization_Level Sector { get; set; }

    [AeFormIgnore]
    public int? BranchId { get; set; }
    [AeFormCategory("Sector Information")]
    [MudForm(IsDropDown = true)]
    [ForeignKey("BranchId")]
    public Organization_Level Branch { get; set; }

    [AeFormIgnore]
    public int? DivisionId { get; set; }

    [AeFormCategory("Sector Information")]
    [MudForm(IsDropDown = true)]
    [ForeignKey("DivisionId")]
    public Organization_Level Division { get; set; }

    [StringLength(4000)]
    [AeFormIgnore]
    [AeLabel(isDropDown: true, placeholder: " ")]
    public string Sector_Name { get; set; }


    [StringLength(200)]
    [AeLabel(isDropDown: true, placeholder: " ")]
    [AeFormIgnore]
    public string Branch_Name { get; set; }

    [AeLabel(isDropDown: true, placeholder: " ")]
    [AeFormIgnore]
    public string Division_Name { get; set; }


    [AeFormCategory("Initiative Information")]
    public string Contact_List { get; set; }

    [StringLength(100)]
    [AeFormCategory("Initiative Information")]
    public string Project_Name { get; set; }

    [StringLength(100)]
    [AeFormCategory("Initiative Information")]
    public string Project_Name_Fr { get; set; }

    [Required]
    [StringLength(10)]
    [AeFormCategory("Initiative Information")]
    public string Project_Acronym_CD { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [AeFormCategory("Initiative Information")]
    public decimal? Project_Budget { get; set; }

    [AeFormCategory("Initiative Information")]
    public string Project_Admin { get; set; }
    [AeFormCategory("Initiative Information")]
    public string Project_Summary_Desc { get; set; }
    [AeFormCategory("Initiative Information")]
    public string Project_Summary_Desc_Fr { get; set; }

    [AeFormCategory("Initiative Information")]
    public string Project_Goal { get; set; }

    [AeFormCategory("Initiative Information")]
    [AeLabel(isDropDown: true)]
    public string Project_Category { get; set; }
    [AeFormCategory("Initiative Information")]
    public DateTime Initial_Meeting_DT { get; set; }

    [AeFormCategory("Initiative Information")]
    public int? Number_Of_Users_Involved { get; set; }
    [AeFormCategory("Initiative Information")]
    public bool Is_Private { get; set; }

    [AeFormCategory("Initiative Information")]
    public bool Is_Featured { get; set; }
    [AeFormCategory("Initiative Information")]
    [Required]
    [AeLabel(validValues: new[] { "Unclassified", "Protected A", "Protected B" })]
    public string Data_Sensitivity { get; set; } = "Unclassified";

    [AeFormCategory("Initiative Information")]
    public string Stage_Desc { get; set; }

    [AeFormIgnore]
    public string Project_Status_Desc { get; set; }

    [AeFormIgnore]
    public int? Project_Status { get; set; }

    [NotMapped]
    [AeFormCategory("Initiative Information")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer Project_Status_Values { get; set; }


    [AeFormCategory("Initiative Information")]
    [AeLabel(isDropDown: true)]
    public string Project_Phase { get; set; }

    [AeFormCategory("Initiative Information")]
    public string GC_Docs_URL { get; set; }

    [AeFormCategory("Initiative Information")]
    public string Project_Icon { get; set; }

    [AeFormIgnore]
    public string Comments_NT { get; set; }
    [AeFormCategory("Initiative Information")]
    public DateTime? Last_Contact_DT { get; set; }
    [AeFormCategory("Initiative Information")]
    public DateTime? Next_Meeting_DT { get; set; }
    [AeFormCategory("Initiative Information")]
    public PBI_License_Request PBI_License_Request { get; set; }

    [AeFormIgnore]
    public DateTime Last_Updated_DT { get; set; }

    [AeFormIgnore]
    public string Last_Updated_UserId { get; set; }


    [AeFormIgnore]
    public DateTime? Deleted_DT { get; set; }

    public List<Datahub_ProjectComment> Comments { get; set; }

    public List<Datahub_Project_User> Users { get; set; }

    public List<Datahub_ProjectServiceRequests> ServiceRequests { get; set; }

    public List<Client_Engagement> Client_Engagements { get; set; }

    public List<Project_Storage_Capacity> Storage_Capacities { get; set; }

    [StringLength(400)]
    [AeFormCategory("Initiative Connections")]
    [Obsolete("Use the new Project Resources relationship instead.", true)]
    public string Databricks_URL { get; set; }

    [AeFormCategory("Initiative Connections")]
    [StringLength(400)]
    public string PowerBI_URL { get; set; }
    [AeFormCategory("Initiative Connections")]
    [StringLength(400)]
    public string WebForms_URL { get; set; }

    public List<WebForm> WebForms { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    [AeFormCategory("Initiative Connections")]
    [StringLength(128)]
    public string DB_Name { get; set; }
    [AeFormCategory("Initiative Connections")]
    [StringLength(128)]
    public string DB_Server { get; set; }
    [AeFormCategory("Initiative Connections")]
    [StringLength(100)]
    [AeLabel(validValues: new[] { SQL_SERVER_DB_TYPE, POSTGRES_DB_TYPE })]
    public string DB_Type { get; set; }

    [AeFormCategory("Initiative Connections")]
    public bool IsDatabasePostgres => DB_Type == POSTGRES_DB_TYPE;
    [AeFormCategory("Initiative Connections")]
    public bool IsDatabaseSqlServer => DB_Type == SQL_SERVER_DB_TYPE;
    [AeFormCategory("Initiative Connections")]
    public bool HasAssociatedDatabase => IsDatabasePostgres || IsDatabaseSqlServer;

    public List<Datahub_Project_Pipeline_Lnk> Pipelines { get; set; }

    public List<Project_Storage> StorageAccounts { get; set; }

    public IList<Project_Resources2> Resources { get; set; }

    public IList<PowerBi_Workspace> PowerBi_Workspaces { get; set; }

    [AeFormIgnore]
    public int OnboardingApplicationId { get; set; }

    [AeFormIgnore]
    public bool? MetadataAdded { get; set; }

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
            return new DatahubProjectInfo(Project_Name, Project_Name_Fr, Project_Acronym_CD);
        }
    }

    public int CompareTo(Datahub_Project other)
    {
        if (Project_Acronym_CD is null || other.Project_Acronym_CD is null)
            return Project_ID.CompareTo(other.Project_ID);
        return Project_Acronym_CD.CompareTo(other.Project_Acronym_CD);
    }


    public string GetProjectStatus()
    {
        if (Project_Status == 2)
        {
            return "Closed";
        }
        else if (Client_Engagements is null)
        {
            return "On Hold";
        }
        else if (Client_Engagements.Where(e => e.Is_Engagement_Active).Any())
        {
            return "In Progress";
        }
        else if (Client_Engagements.Any())
        {
            return "Support";
        }

        return "On Hold";
    }

    [AeFormIgnore]
    public TerraformWorkspace ToResourceWorkspace(List<TerraformUser> users)
    {
        return new TerraformWorkspace()
        {
            Name = Project_Name,
            Acronym = Project_Acronym_CD,
            TerraformOrganization = new TerraformOrganization()
            {
                Name = Branch_Name ?? "TODO",
                Code = "TODO"
            },
            Users = users
        };
    }
}
