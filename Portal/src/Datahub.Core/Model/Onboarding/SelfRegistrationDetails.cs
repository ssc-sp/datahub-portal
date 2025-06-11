namespace Datahub.Core.Model.Onboarding;

/// <summary>
/// Represents the details captured during a user's self-registration process.
/// </summary>
public class SelfRegistrationDetails
{
    /// <summary>
    /// Gets or sets the unique identifier for the self-registration entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the email address provided by the user during registration.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets any comments or additional information provided by the user during registration.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the self-registration entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}