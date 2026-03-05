using Datahub.Core.Model.Users;

namespace Datahub.Application.Commands;

public class ProjectUserAddExternalUserCommand
{
    public string ProjectAcronym { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Affiliation { get; set; }
    public string Organization { get; set; }
    public DateTime AccountExpiry { get; set; }
    public string CollaborationObjectives { get; set; }
    public int RoleId { get; set; }

    public ExternalUser? ExternalUser { get; set; }
}
