using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Data;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Repositories;
using Datahub.Shared.Entities;
using Elemental.Components;
using MudBlazor.Forms;
using AeFormCategoryAttribute = MudBlazor.Forms.AeFormCategoryAttribute;
using AeFormIgnoreAttribute = MudBlazor.Forms.AeFormIgnoreAttribute;

namespace Datahub.Core.Model.Projects;

public enum ProjectStatus
{
    OnHold = 0,
    InProgress = 1,
    Support = 2,
    Closed = 3
}
public class DatahubProject : IComparable<DatahubProject>
{
    public const string CLOSED = "Closed";
    public const string ONHOLD = "On Hold";

    public const string SQLSERVERDBTYPE = "SQL Server";
    public const string POSTGRESDBTYPE = "Postgres";

    [AeFormIgnore]
    [Key]
    public int ProjectID { get; set; }

    [AeFormIgnore]
    public int? SectorId { get; set; }

    [AeFormCategory("Sector Information")]
    [MudForm(IsDropDown = true)]
    [ForeignKey("SectorId")]
    public OrganizationLevel Sector { get; set; }

    [AeFormIgnore]
    public int? BranchId { get; set; }
    [AeFormCategory("Sector Information")]
    [MudForm(IsDropDown = true)]
    [ForeignKey("BranchId")]
    public OrganizationLevel Branch { get; set; }

    [AeFormIgnore]
    public int? DivisionId { get; set; }

    [AeFormCategory("Sector Information")]
    [MudForm(IsDropDown = true)]
    [ForeignKey("DivisionId")]
    public OrganizationLevel Division { get; set; }

    [StringLength(4000)]
    [AeFormIgnore]
    [AeLabel(isDropDown: true, placeholder: " ")]
    public string SectorName { get; set; }

    [StringLength(200)]
    [AeLabel(isDropDown: true, placeholder: " ")]
    [AeFormIgnore]
    public string BranchName { get; set; }

    [AeLabel(isDropDown: true, placeholder: " ")]
    [AeFormIgnore]
    public string DivisionName { get; set; }

    [AeFormCategory("Initiative Information")]
    public string ContactList { get; set; }

    [StringLength(100)]
    [AeFormCategory("Initiative Information")]
    public string Project_Name { get; set; }

    [StringLength(100)]
    [AeFormCategory("Initiative Information")]
    public string ProjectNameFr { get; set; }

    [Required]
    [StringLength(10)]
    [AeFormCategory("Initiative Information")]
    public string ProjectAcronymCD { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [AeFormCategory("Initiative Information")]
    public decimal? ProjectBudget { get; set; }

    [AeFormCategory("Initiative Information")]
    public string ProjectAdmin { get; set; }
    [AeFormCategory("Initiative Information")]
    public string ProjectSummaryDesc { get; set; }
    [AeFormCategory("Initiative Information")]
    public string ProjectSummaryDescFr { get; set; }

    [AeFormCategory("Initiative Information")]
    public string ProjectGoal { get; set; }

    [AeFormCategory("Initiative Information")]
    [AeLabel(isDropDown: true)]
    public string ProjectCategory { get; set; }
    [AeFormCategory("Initiative Information")]
    public DateTime InitialMeetingDT { get; set; }

    [AeFormCategory("Initiative Information")]
    public int? NumberOfUsersInvolved { get; set; }
    [AeFormCategory("Initiative Information")]
    public bool IsPrivate { get; set; }

    [AeFormCategory("Initiative Information")]
    public bool IsFeatured { get; set; }
    [AeFormCategory("Initiative Information")]
    [Required]
    [AeLabel(validValues: new[] { "Unclassified", "Protected A", "Protected B" })]
    public string DataSensitivity { get; set; } = "Unclassified";

    [AeFormCategory("Initiative Information")]
    public string StageDesc { get; set; }

    [AeFormIgnore]
    public string ProjectStatusDesc { get; set; }

    [AeFormIgnore]
    public int? ProjectStatus { get; set; }

    [NotMapped]
    [AeFormCategory("Initiative Information")]
    [MudForm(IsDropDown = true)]
    public DropDownContainer ProjectStatusValues { get; set; }

    [AeFormCategory("Initiative Information")]
    [AeLabel(isDropDown: true)]
    public string ProjectPhase { get; set; }

    [AeFormCategory("Initiative Information")]
    public string GCDocsURL { get; set; }

    [AeFormCategory("Initiative Information")]
    public string ProjectIcon { get; set; }

    [AeFormIgnore]
    public string CommentsNT { get; set; }
    [AeFormCategory("Initiative Information")]
    public DateTime? LastContactDT { get; set; }
    [AeFormCategory("Initiative Information")]
    public DateTime? NextMeetingDT { get; set; }
    [AeFormCategory("Initiative Information")]
    public PBILicenseRequest PBILicenseRequest { get; set; }

    [AeFormIgnore]
    public DateTime LastUpdatedDT { get; set; }

    [AeFormIgnore]
    public string LastUpdatedUserId { get; set; }

    [AeFormIgnore]
    public DateTime? DeletedDT { get; set; }

    [AeFormIgnore]
    public bool IsDeleted => DeletedDT != null && DeletedDT < DateTime.UtcNow;

    public List<DatahubProjectComment> Comments { get; set; }

    public List<DatahubProjectUser> Users { get; set; }

    public List<ClientEngagement> ClientEngagements { get; set; }

    public ProjectCredits Credits { get; set; }

    public ProjectWhitelist Whitelist { get; set; }

    public List<ProjectInactivityNotifications> ProjectInactivityNotifications { get; set; }

    [StringLength(400)]
    [AeFormCategory("Initiative Connections")]
    [Obsolete("Use the new Project Resources relationship instead.", true)]
    public string DatabricksURL { get; set; }

    [AeFormCategory("Initiative Connections")]
    [StringLength(400)]
    public string PowerBIURL { get; set; }
    [AeFormCategory("Initiative Connections")]
    [StringLength(400)]
    public string WebFormsURL { get; set; }

    public List<WebForm> WebForms { get; set; }

    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    [AeFormCategory("Initiative Connections")]
    [StringLength(128)]
    public string DBName { get; set; }
    [AeFormCategory("Initiative Connections")]
    [StringLength(128)]
    public string DBServer { get; set; }
    [AeFormCategory("Initiative Connections")]
    [StringLength(100)]
    [AeLabel(validValues: new[] { SQLSERVERDBTYPE, POSTGRESDBTYPE })]
    public string DBType { get; set; }

    [AeFormCategory("Initiative Connections")]
    public bool IsDatabasePostgres => DBType == POSTGRESDBTYPE;
    [AeFormCategory("Initiative Connections")]
    public bool IsDatabaseSqlServer => DBType == SQLSERVERDBTYPE;
    [AeFormCategory("Initiative Connections")]
    public bool HasAssociatedDatabase => IsDatabasePostgres || IsDatabaseSqlServer;

    public List<DatahubProjectPipelineLnk> Pipelines { get; set; }

    public IList<ProjectResources2> Resources { get; set; }

    public IList<PowerBiWorkspace> PowerBiWorkspaces { get; set; }

    public List<ProjectRepository> Repositories { get; set; }

    [AeFormIgnore]
    public int OnboardingApplicationId { get; set; }

    [AeFormIgnore]
    public bool? MetadataAdded { get; set; }

    [AeFormIgnore]
    public bool? WebAppEnabled { get; set; }

    [AeFormIgnore]
    public DateTime? LastLoginDate
    {
        get
        {
            if (Users != null)
            {
                return Users.Select(x => x.PortalUser.LastLoginDateTime).Max();
            }
            return LastUpdatedDT;
        }
    }

    [AeFormIgnore]
    public DateTime? OperationalWindow { get; set; }

    private bool hasCostRecovery;
    [AeFormIgnore]
    public bool HasCostRecovery
    {
        get
        {
            return hasCostRecovery || (ProjectID <= 42);
        }
        set
        {
            hasCostRecovery = value;
        }
    }

    [AeFormIgnore]
    [StringLength(128)]
    public string WebAppURL { get; set; }

    [AeFormIgnore]
    [StringLength(16)]
    public string Version { get; set; } = TerraformWorkspace.DefaultVersion;

    [AeFormIgnore]
    [StringLength(150)]
    public string GitRepoURL { get; set; }

    public List<ProjectCloudStorage> CloudStorages { get; set; }

    [AeFormIgnore]
    [NotMapped]
    public string ProjectName
    {
        get
        {
            if (Thread.CurrentThread.CurrentCulture.Name.Equals("fr-ca", StringComparison.OrdinalIgnoreCase))
            {
                return !string.IsNullOrWhiteSpace(ProjectNameFr) ? ProjectNameFr : Project_Name + " (*)";
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
                return !string.IsNullOrWhiteSpace(ProjectSummaryDescFr) ? ProjectSummaryDescFr : ProjectSummaryDesc;
            }
            return ProjectSummaryDesc;
        }
    }

    [AeFormIgnore]
    [NotMapped]
    public DatahubProjectInfo ProjectInfo
    {
        get
        {
            return new DatahubProjectInfo(Project_Name, ProjectNameFr, ProjectAcronymCD);
        }
    }

    public string HashedAPIToken { get; set; }

    public DateTime ExpiryDate { get; set; }

    public int CompareTo(DatahubProject other)
    {
        if (ProjectAcronymCD is null || other.ProjectAcronymCD is null)
            return ProjectID.CompareTo(other.ProjectID);
        return ProjectAcronymCD.CompareTo(other.ProjectAcronymCD);
    }

    public string GetProjectStatus()
    {
        if (ProjectStatus == 2)
        {
            return "Closed";
        }
        else if (ClientEngagements is null)
        {
            return "On Hold";
        }
        else if (ClientEngagements.Where(e => e.IsEngagementActive).Any())
        {
            return "In Progress";
        }
        else if (ClientEngagements.Any())
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
            Acronym = ProjectAcronymCD,
            BudgetAmount = Convert.ToDouble(ProjectBudget),
            Version = Version ?? TerraformWorkspace.DefaultVersion,
            TerraformOrganization = new TerraformOrganization()
            {
                Name = BranchName ?? "TODO",
                Code = "TODO"
            },
            Users = users
        };
    }
}
