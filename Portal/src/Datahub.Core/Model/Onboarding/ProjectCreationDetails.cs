using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Model.Onboarding;

/// <summary>
/// This class is used to store the questions and answers for the project creation form.
/// </summary>
public class ProjectCreationDetails
{
    /// <summary>
    /// Gets or sets the id of the project creation details.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the id of the project that the user has created.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the joined string of the features that the user is interested in.
    /// </summary>
    public string InterestedFeatures { get; set; }

    /// <summary>
    /// Gets or sets the id of the user that is creating the details for the project.
    /// </summary>
    public int CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the date and time that the project creation details were created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    #region Navigation

    public Datahub_Project Project { get; set; } = null!;
    public PortalUser CreatedBy { get; set; } = null!;

    #endregion
}