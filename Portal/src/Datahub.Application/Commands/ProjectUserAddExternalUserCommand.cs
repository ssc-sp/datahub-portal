using Datahub.Core.Model.Projects;
using Datahub.Core.Model.Users;

namespace Datahub.Application.Commands;

public class ProjectUserAddExternalUserCommand
{
    public required string ProjectAcronym { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Affiliation { get; set; }
    public required string Organization { get; set; }
    public required DateTime AccountExpiry { get; set; }
    public required string CollaborationObjectives { get; set; }
    public required Project_Role Role { get; set; }

    public ExternalUser? ExternalUser { get; set; }
}
