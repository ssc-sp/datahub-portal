using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Datahub.Core.Data;
using Datahub.Core.Model.CloudStorage;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Onboarding;
using Datahub.Core.Model.Repositories;
using Datahub.Core.Model.Subscriptions;
using Datahub.Shared.Entities;
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

public enum VersionUpdateType : int
{
    Major,
    Minor,
    Build,
    None
}

/// <summary>
/// Represents a Datahub workspace, encapsulating all its associated information and resources.
/// </summary>
public class Datahub_Project : IComparable<Datahub_Project>
{
    public const string CLOSED = "Closed";
    public const string ON_HOLD = "On Hold";

    public const string SQL_SERVER_DB_TYPE = "SQL Server";
    public const string POSTGRES_DB_TYPE = "Postgres";

    /// <summary>
    /// Gets or sets the unique identifier for the workspace.
    /// </summary>
    [AeFormIgnore]
    [Key]
    public int Project_ID { get; set; }

    /// <summary>
    /// Gets or sets the email for the user who created the workspace.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    public string Contact_List { get; set; }

    /// <summary>
    /// Gets or sets the English name of the workspace.
    /// </summary>
    [StringLength(100)]
    [AeFormCategory("Workspace Information")]
    public string Project_Name { get; set; }

    /// <summary>
    /// Gets or sets the French name of the workspace.
    /// </summary>
    [StringLength(100)]
    [AeFormCategory("Workspace Information")]
    public string Project_Name_Fr { get; set; }

    /// <summary>
    /// Gets or sets the acronym code for the workspace.
    /// </summary>
    [Required]
    [StringLength(10)]
    [AeFormCategory("Workspace Information")]
    public string Project_Acronym_CD { get; set; }

    /// <summary>
    /// Gets or sets the budget allocated to the workspace.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    [AeFormCategory("Workspace Information")]
    public decimal? Project_Budget { get; set; } = 0;

    /// <summary>
    /// Gets or sets the administrator of the workspace. Same as the owner today.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    public string Project_Admin { get; set; }

    /// <summary>
    /// Gets or sets the summary description of the workspace in English.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    public string Project_Summary_Desc { get; set; }

    /// <summary>
    /// Gets or sets the summary description of the workspace in French.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    public string Project_Summary_Desc_Fr { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the workspace is private.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    public bool Is_Private { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the workspace is featured.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    public bool Is_Featured { get; set; }

    /// <summary>
    /// Gets or sets the data sensitivity classification for the workspace.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    [Required]
    [MudForm(ValidValues= new[] { "Unclassified", "Protected A", "Protected B" })]
    public string Data_Sensitivity { get; set; } = "Unclassified";

    /// <summary>
    /// Gets or sets the description of the workspace's status.
    /// </summary>
    [AeFormIgnore]
    public string Project_Status_Desc { get; set; }

    /// <summary>
    /// Gets or sets the numerical status of the workspace, corresponding to the <see cref="ProjectStatus"/> enum.
    /// </summary>
    [AeFormIgnore]
    public int? Project_Status { get; set; }

    /// <summary>
    /// Gets or sets the phase of the workspace.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    [MudForm(IsDropDown=true)]
    public string Project_Phase { get; set; }

    /// <summary>
    /// Gets or sets the icon for the workspace.
    /// </summary>
    [AeFormCategory("Workspace Information")]
    public string Project_Icon { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the workspace was last updated.
    /// </summary>
    [AeFormIgnore]
    public DateTime Last_Updated_DT { get; set; }

    /// <summary>
    /// Gets or sets the user ID of the person who last updated the workspace.
    /// </summary>
    [AeFormIgnore]
    public string Last_Updated_UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the workspace was marked as deleted. Null if not deleted.
    /// </summary>
    [AeFormIgnore]
    public DateTime? Deleted_DT { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the workspace was created.
    /// </summary>
    [AeFormIgnore]
    public DateTime Created_DT { get; set; }

    /// <summary>
    /// Gets or sets the Id of the Azure subscription of the workspace in Datahub.
    /// </summary>
    public int DatahubAzureSubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the Azure subscription of the workspace in Datahub.
    /// </summary>
    public DatahubAzureSubscription DatahubAzureSubscription { get; set; }

    /// <summary>
    /// Gets a value indicating whether the workspace is marked as deleted.
    /// </summary>
    [AeFormIgnore]
    public bool IsDeleted => Deleted_DT != null && Deleted_DT < DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the list of roles and users associated with the workspace.
    /// </summary>
    public List<UserRoleLinks> UserRoles { get; set; }

    /// <summary>
    /// Gets or sets the credits for the workspace.
    /// </summary>
    public Project_Credits Credits { get; set; }

    /// <summary>
    /// Gets or sets the whitelist for the workspace.
    /// </summary>
    public Project_Whitelist Whitelist { get; set; }

    /// <summary>
    /// Gets or sets the list of inactivity notifications for the workspace.
    /// </summary>
    public List<ProjectInactivityNotifications> ProjectInactivityNotifications { get; set; }

    /// <summary>
    /// Gets a value indicating whether the workspace is over budget.
    /// </summary>
    public bool IsOverBudget => Credits is null ? false : Credits.Current >= (double)Project_Budget;

    /// <summary>
    /// Gets or sets the timestamp for concurrency control.
    /// </summary>
    [AeFormIgnore]
    [Timestamp]
    public byte[] Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the database type for the workspace.
    /// </summary>
    [AeFormCategory("Initiative Connections")]
    [StringLength(100)]
    [MudForm(ValidValues= new[] { SQL_SERVER_DB_TYPE, POSTGRES_DB_TYPE })]
    public string DB_Type { get; set; }

    /// <summary>
    /// Gets a value indicating whether the workspace uses a Postgres database.
    /// </summary>
    [AeFormCategory("Initiative Connections")]
    public bool IsDatabasePostgres => DB_Type == POSTGRES_DB_TYPE;

    /// <summary>
    /// Gets a value indicating whether the workspace uses a SQL Server database.
    /// </summary>
    [AeFormCategory("Initiative Connections")]
    public bool IsDatabaseSqlServer => DB_Type == SQL_SERVER_DB_TYPE;

    /// <summary>
    /// Gets or sets the list of resources associated with the workspace.
    /// </summary>
    public IList<Project_Resources2> Resources { get; set; }

    /// <summary>
    /// Gets or sets the list of repositories associated with the workspace.
    /// </summary>
    public List<ProjectRepository> Repositories { get; set; }

    /// <summary>
    /// Gets or sets the list of publishing submissions for the workspace.
    /// </summary>
    public IList<OpenDataSubmission> PublishingSubmissions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether metadata has been added to the workspace.
    /// </summary>
    [AeFormIgnore]
    public bool? MetadataAdded { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a web application is enabled for the workspace.
    /// </summary>
    [AeFormIgnore]
    public bool? WebAppEnabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether URL rewriting is enabled for the web application of the workspace.
    /// </summary>
    [AeFormIgnore]
    public bool WebAppUrlRewritingEnabled { get; set; }

    /// <summary>
    /// Gets the last login date of any user associated with the workspace.
    /// Returns the last updated date if no users are present.
    /// </summary>
    [AeFormIgnore]
    public DateTime? LastLoginDate
    {
        get
        {
            if (UserRoles != null)
            {
                return UserRoles.Select(x => x.PortalUser.LastLoginDateTime).Max();
            }
            return Last_Updated_DT;
        }
    }

    /// <summary>
    /// Gets or sets the operational window for the workspace.
    /// </summary>
    [AeFormIgnore]
    public DateTime? OperationalWindow { get; set; }

    /// <summary>
    /// Gets or sets the URL for the web application associated with the workspace.
    /// </summary>
    [AeFormIgnore]
    [StringLength(128)]
    public string WebApp_URL { get; set; }

    /// <summary>
    /// Gets or sets the version of the workspace, typically related to its Terraform configuration.
    /// </summary>
    [AeFormIgnore]
    [StringLength(16)]
    public string Version { get; set; } = TerraformWorkspace.DefaultVersion;

    /// <summary>
    /// Gets or sets the URL of the Git repository associated with the workspace.
    /// </summary>
    [AeFormIgnore]
    [StringLength(150)]
    public string GitRepo_URL { get; set; }

    /// <summary>
    /// Gets or sets the list of cloud storage resources for the workspace.
    /// </summary>
    public List<ProjectCloudStorage> CloudStorages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the project should be prevented from auto-deletion.
    /// </summary>
    public bool PreventAutoDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets the date when the project is granting access to Datahub support.
    /// </summary>
    public DateTime AllowDatahubSupport { get; set; }

    /// <summary>
    /// Gets the localized project name based on the current thread's culture.
    /// Appends " (*)" to the English name if the French name is not available and the culture is French.
    /// </summary>
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

    /// <summary>
    /// Gets the localized project description based on the current thread's culture.
    /// Returns the English description if the French description is not available and the culture is French.
    /// </summary>
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

    /// <summary>
    /// Gets the project information including English name, French name, and acronym.
    /// </summary>
    [AeFormIgnore]
    [NotMapped]
    public DatahubProjectInfo ProjectInfo
    {
        get
        {
            return new DatahubProjectInfo(Project_Name, Project_Name_Fr, Project_Acronym_CD);
        }
    }

    /// <summary>
    /// Gets or sets the hashed API token for the workspace. This is for future use and is not currently used.
    /// </summary>
    public string HashedAPIToken { get; set; }

    /// <summary>
    /// Gets or sets the expiry date for the workspace.
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    public int CompareTo(Datahub_Project other)
    {
        if (Project_Acronym_CD is null || other.Project_Acronym_CD is null)
            return Project_ID.CompareTo(other.Project_ID);
        return Project_Acronym_CD.CompareTo(other.Project_Acronym_CD);
    }

    /// <summary>
    /// Gets or sets the parent GC Hosting budget ID for the workspace.
    /// </summary>
    public int? ParentGCHostingBudgetId { get; set; }

    /// <summary>
    /// Gets or sets the parent GC Hosting budget details for the workspace.
    /// </summary>
    public GCHostingWorkspaceDetails ParentGCHostingBudget { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an update has been requested for the workspace.
    /// </summary>
    public bool IsVersionUpdateRequested { get; set; }

    /// <summary>
    /// Converts a Datahub_Project object to a TerraformWorkspace object.
    /// </summary>
    /// <param name="users">The list of TerraformUser objects.</param>
    /// <exception cref="InvalidOperationException">Throws an exception when the Datahub Azure Subscription is not included</exception>
    /// <returns>A TerraformWorkspace object populated with values from the Datahub_Project object.</returns>
    public TerraformWorkspace ToResourceWorkspace(List<TerraformUser> users)
    {
        return new TerraformWorkspace()
        {
            Name = Project_Name,
            Acronym = Project_Acronym_CD,
            SSCCBRID = ParentGCHostingBudget?.CBRID ?? string.Empty,
            BudgetAmount = Convert.ToDouble(Project_Budget),
            Version = Version ?? TerraformWorkspace.DefaultVersion,
            TerraformOrganization = new TerraformOrganization()
            {
                Name = "SSC",
                Code = "SSC"
            },
            Users = users,
            SubscriptionId = DatahubAzureSubscription?.SubscriptionId ?? throw new InvalidOperationException("Azure subscription not found.")
        };
    }

    public VersionUpdateType GetUpdateType(string latestVersion)
    {
        if (Version != null && Version != "latest")
        {
            var inputVersion = System.Version.Parse(Version.TrimStart('v'));
            var latestParsedVersion = System.Version.Parse(latestVersion.TrimStart('v'));

            if (inputVersion.Major < latestParsedVersion.Major)
            {
                return VersionUpdateType.Major;
            }

            if (inputVersion.Minor < latestParsedVersion.Minor)
            {
                return VersionUpdateType.Minor;
            }
        }

        return VersionUpdateType.None;
    }
}
