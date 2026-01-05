using Datahub.Core.Model.Projects;
using Datahub.Metadata.Model;

namespace Datahub.Core.Model.Onboarding;

/// <summary>
/// Represents the details of a workspace received from the GC Hosting onboarding process.
/// </summary>
#nullable enable
public class GCHostingWorkspaceDetails
{
    /// <summary>
    /// Gets or sets the id of the workspace (internal GC Hosting identifier).
    /// </summary>
    public string GcHostingId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the id of the workspace.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///  Gets or sets the first name of the workspace lead.
    /// </summary>
    public string LeadFirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name of the workspace lead.
    /// </summary>
    public string LeadLastName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the department or agency of the workspace.
    /// </summary>
    public string DepartmentName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the government email of the workspace lead.
    /// </summary>
    public string LeadEmail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the first name of the financial authority.
    /// </summary>
    public string FinancialAuthorityFirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name of the financial authority.
    /// </summary>
    public string FinancialAuthorityLastName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the financial authority commitment isref.
    /// </summary>
    public string FinancialAuthorityCommitmentIsRef { get; set; } = null!;

    /// <summary>
    /// Gets or sets the financial authority commitment isorg.
    /// </summary>
    public string FinancialAuthorityCommitmentIsOrg { get; set; } = null!;

    /// <summary>
    /// Gets or sets the government email of the financial authority.
    /// </summary>
    public string FinancialAuthorityEmail { get; set; } = null!;

    /// <summary>
    /// Gets or sets the budget of the workspace.
    /// </summary>
    public decimal WorkspaceBudget { get; set; }

    /// <summary>
    /// Gets or sets the workspace title.
    /// </summary>
    public string WorkspaceName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the workspace description.
    /// </summary>
    public string WorkspaceDescription { get; set; } = null!;

    /// <summary>
    /// Gets or sets the subject of the workspace.
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// Gets or sets keywords for the workspace.
    /// </summary>
    public string Keywords { get; set; } = null!;

    /// <summary>
    /// Gets or sets the retention period in years for the workspace.
    /// </summary>
    public int RetentionPeriodYears { get; set; }

    /// <summary>
    /// Gets or sets the retention period start date for the workspace.
    /// </summary>
    public DateTime RetentionPeriodStartDate { get; set; }

    /// <summary>
    /// Gets or sets the retention value for the workspace.
    /// </summary>
    public string RetentionValue { get; set; } = null!;

    /// <summary>
    /// Gets or sets a value indicating whether the workspace generates business value.
    /// </summary>
    public bool GeneratesInfoBusinessValue { get; set; }

    /// <summary>
    /// Gets or sets the security classification of the workspace.
    /// </summary>
    public ClassificationType SecurityClassification { get; set; }

    /// <summary>
    /// Gets or sets the project title.
    /// </summary>
    public string? ProjectTitle { get; set; } = null;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string? ProjectDescription { get; set; } = null;

    /// <summary>
    /// Gets or sets the CBR name.
    /// </summary>
    public string CBRName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the CBR ID.
    /// </summary>
    public string CBRID { get; set; } = null!;

    public IEnumerable<Datahub_Project> WorkspacesInBudget { get; set; } = new List<Datahub_Project>();
}
#nullable disable
