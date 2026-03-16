using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Application.Commands;

public class ProjectUserAddExternalUserCommand
{
    public ExternalUser? ExternalUser { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    public Project_Role Role { get; set; } = null!;

    [Required(ErrorMessage = "Organization is required.")]
    public string Organization { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expiry date is required.")]
    public DateTime? AccountExpiry { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Collaboration objectives are required.")]
    public string CollaborationObjectives { get; set; } = string.Empty;

    public required string ProjectAcronym { get; set; }
}
