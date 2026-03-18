using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;
using System.ComponentModel.DataAnnotations;

namespace Datahub.Application.Commands;

public class ProjectUserAddExternalUserCommand : IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasMinimumTrimmedLength(FirstName, 2))
        {
            yield return new ValidationResult("First name must be at least 2 characters.", [nameof(FirstName)]);
        }

        if (!HasMinimumTrimmedLength(LastName, 2))
        {
            yield return new ValidationResult("Last name must be at least 2 characters.", [nameof(LastName)]);
        }

        if (!HasMinimumTrimmedLength(Organization, 2))
        {
            yield return new ValidationResult("Organization must be at least 2 characters.", [nameof(Organization)]);
        }
    }

    private static bool HasMinimumTrimmedLength(string? value, int minimumLength)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Trim().Length >= minimumLength;
    }
}
