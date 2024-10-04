using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Onboarding;

public class GCHostingWorkspaceDetails
{
    /// <summary>
    /// Gets or sets the id of the workspace.
    /// </summary>
    public string GcHostingId { get; set; }

    /// <summary>
    /// Gets or sets the id of the workspace.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///  Gets or sets the first name of the workspace lead.
    /// </summary>
    public string LeadFirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the workspace lead.
    /// </summary>
    public string LeadLastName { get; set; }

    /// <summary>
    /// Gets or sets the department or agency of the workspace.
    /// </summary>
    public string DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets the government email of the workspace lead.
    /// </summary>
    public string LeadEmail { get; set; }

    /// <summary>
    /// Gets or sets the first name of the financial authority.
    /// </summary>
    public string FinancialAuthorityFirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the financial authority.
    /// </summary>
    public string FinancialAuthorityLastName { get; set; }

    /// <summary>
    /// Gets or sets the cost centre of the financial authority.
    /// </summary>
    public string FinancialAuthorityCostCentre { get; set; }

    /// <summary>
    /// Gets or sets the workspace title.
    /// </summary>
    public string WorkspaceTitle { get; set; }

    /// <summary>
    /// Gets or sets the workspace description.
    /// </summary>
    public string WorkspaceDescription { get; set; }

    /// <summary>
    /// Gets or sets the workspace identifier.
    /// </summary>
    public string WorkspaceIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the subject of the workspace.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets keywords for the workspace.
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// Gets or sets the area of science for the workspace.
    /// </summary>
    public string AreaOfScience { get; set; }

    /// <summary>
    /// Gets or sets the retention period in years for the workspace.
    /// </summary>
    public int RetentionPeriodYears { get; set; }

    /// <summary>
    /// Gets or sets the security classification of the workspace.
    /// </summary>
    public string SecurityClassification { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the workspace generates business value.
    /// </summary>
    public bool GeneratesInfoBusinessValue { get; set; }

    /// <summary>
    /// Gets or sets the project title.
    /// </summary>
    public string ProjectTitle { get; set; }

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string ProjectDescription { get; set; }

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    public DateTime ProjectStartDate { get; set; }

    /// <summary>
    /// Gets or sets the project end date.
    /// </summary>
    public DateTime ProjectEndDate { get; set; }

    /// <summary>
    /// Gets or sets the CBR name.
    /// </summary>
    public string CBRName { get; set; }

    /// <summary>
    /// Gets or sets the CBR ID.
    /// </summary>
    public string CBRID { get; set; }

    /// <summary>
    /// Gets or sets the project that the workspace is associated with.
    /// </summary>
    public Datahub_Project Datahub_Project { get; set; }
}