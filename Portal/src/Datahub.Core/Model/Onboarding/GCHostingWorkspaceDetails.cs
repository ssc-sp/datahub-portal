using System.Text.Json.Serialization;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Onboarding;

public class GCHostingWorkspaceDetails
{
    /// <summary>
    /// Gets or sets the id of the workspace.
    /// </summary>
    [JsonPropertyName("GcHostingId")]
    public string GcHostingId { get; set; }

    /// <summary>
    /// Gets or sets the id of the workspace.
    /// </summary>
    [JsonPropertyName("Id")]
    public int Id { get; set; }

    /// <summary>
    ///  Gets or sets the first name of the workspace lead.
    /// </summary>
    [JsonPropertyName("LeadFirstName")]
    public string LeadFirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the workspace lead.
    /// </summary>
    [JsonPropertyName("LeadLastName")]
    public string LeadLastName { get; set; }

    /// <summary>
    /// Gets or sets the department or agency of the workspace.
    /// </summary>
    [JsonPropertyName("DepartmentName")]
    public string DepartmentName { get; set; }

    /// <summary>
    /// Gets or sets the government email of the workspace lead.
    /// </summary>
    [JsonPropertyName("LeadEmail")]
    public string LeadEmail { get; set; }

    /// <summary>
    /// Gets or sets the first name of the financial authority.
    /// </summary>
    [JsonPropertyName("FinancialAuthorityFirstName")]
    public string FinancialAuthorityFirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the financial authority.
    /// </summary>
    [JsonPropertyName("FinancialAuthorityLastName")]
    public string FinancialAuthorityLastName { get; set; }

    /// <summary>
    /// Gets or sets the cost centre of the financial authority.
    /// </summary>
    [JsonPropertyName("FinancialAuthorityCostCentre")]
    public string FinancialAuthorityCostCentre { get; set; }

    /// <summary>
    /// Gets or sets the financial authority commitment isref.
    /// </summary>
    public string FinancialAuthorityCommitmentIsRef { get; set; }

    /// <summary>
    /// Gets or sets the financial authority commitment isorg.
    /// </summary>
    public string FinancialAuthorityCommitmentIsOrg { get; set; }

    /// <summary>
    /// Gets or sets the government email of the financial authority.
    /// </summary>
    [JsonPropertyName("FinancialAuthorityEmail")]
    public string FinancialAuthorityEmail { get; set; }

    /// <summary>
    /// Gets or sets the budget of the workspace.
    /// </summary>
    [JsonPropertyName("WorkspaceBudget")]
    public decimal WorkspaceBudget { get; set; }

    /// <summary>
    /// Gets or sets the workspace title.
    /// </summary>
    [JsonPropertyName("WorkspaceTitle")]
    public string WorkspaceTitle { get; set; }

    /// <summary>
    /// Gets or sets the workspace description.
    /// </summary>
    [JsonPropertyName("WorkspaceDescription")]
    public string WorkspaceDescription { get; set; }

    /// <summary>
    /// Gets or sets the workspace identifier.
    /// </summary>
    [JsonPropertyName("WorkspaceIdentifier")]
    public string WorkspaceIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the subject of the workspace.
    /// </summary>
    [JsonPropertyName("Subject")]
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets keywords for the workspace.
    /// </summary>
    [JsonPropertyName("Keywords")]
    public string Keywords { get; set; }

    /// <summary>
    /// Gets or sets the area of science for the workspace.
    /// </summary>
    [JsonPropertyName("AreaOfScience")]
    public string AreaOfScience { get; set; }

    /// <summary>
    /// Gets or sets the retention period in years for the workspace.
    /// </summary>
    [JsonPropertyName("RetentionPeriodYears")]
    public int RetentionPeriodYears { get; set; }

    /// <summary>
    /// Gets or sets the retention period start date for the workspace.
    /// </summary>
    [JsonPropertyName("RetentionPeriodStartDate")]
    public DateTime RetentionPeriodStartDate { get; set; }

    /// <summary>
    /// Gets or sets the retention value for the workspace.
    /// </summary>
    [JsonPropertyName("RetentionValue")]
    public string RetentionValue { get; set; }

    /// <summary>
    /// Gets or sets the security classification of the workspace.
    /// </summary>
    [JsonPropertyName("SecurityClassification")]
    public string SecurityClassification { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the workspace generates business value.
    /// </summary>
    [JsonPropertyName("GeneratesInfoBusinessValue")]
    public bool GeneratesInfoBusinessValue { get; set; }

    /// <summary>
    /// Gets or sets the project title.
    /// </summary>
    [JsonPropertyName("ProjectTitle")]
    public string ProjectTitle { get; set; }

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    [JsonPropertyName("ProjectDescription")]
    public string ProjectDescription { get; set; }

    /// <summary>
    /// Gets or sets the project start date.
    /// </summary>
    [JsonPropertyName("ProjectStartDate")]
    public DateTime ProjectStartDate { get; set; }

    /// <summary>
    /// Gets or sets the project end date.
    /// </summary>
    [JsonPropertyName("ProjectEndDate")]
    public DateTime ProjectEndDate { get; set; }

    /// <summary>
    /// Gets or sets the CBR name.
    /// </summary>
    [JsonPropertyName("CBRName")]
    public string CBRName { get; set; }

    /// <summary>
    /// Gets or sets the CBR ID.
    /// </summary>
    [JsonPropertyName("CBRID")]
    public string CBRID { get; set; }

    /// <summary>
    /// Gets or sets the project that the workspace is associated with.
    /// </summary>
    [JsonPropertyName("Datahub_Project")]
    public Datahub_Project Datahub_Project { get; set; }
}
