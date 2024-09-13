namespace Datahub.Core.Model.Projects;

public class Project_Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    /// <summary>
    /// Gets a value indicating whether this should be used to determine if the user is at least an admin or not.
    /// Note: If you are looking to show or hide UI elements off this, use the "DatahubAuthView" instead.
    /// Note: This will not work inside EF Core Queries
    /// </summary>
    public bool IsAtLeastAdmin => Id is (int)RoleNames.Admin or (int)RoleNames.WorkspaceLead;

    /// <summary>
    /// Gets a value indicating whether this should be used to determine if the user is at least a collaborator or not.
    /// Note: If you are looking to show or hide UI elements off this, use the "DatahubAuthView" instead.
    /// Note: This will not work inside EF Core Queries
    /// </summary>
    public bool IsAtLeastCollaborator => Id is (int)RoleNames.Collaborator || IsAtLeastAdmin;

    /// <summary>
    /// Gets a value indicating whether this should be used to determine if the user is at least a guest or not.
    /// Note: If you are looking to show or hide UI elements off this, use the "DatahubAuthView" instead.
    /// Note: This will not work inside EF Core Queries
    /// </summary>
    public bool IsAtLeastGuest => Id is (int)RoleNames.Guest || IsAtLeastCollaborator;

    public static IEnumerable<Project_Role> GetAll() => roles.Value;

    private static Lazy<IEnumerable<Project_Role>> roles = new(CreateRoles);

    private static IEnumerable<Project_Role> CreateRoles()
    {
        return new List<Project_Role>
        {
            new()
            {
                Id = (int)RoleNames.Remove,
                Name = "Remove User",
                Description = "Revoke the user's access to the workspace"
            },
            new()
            {
                Id = (int)RoleNames.WorkspaceLead,
                Name = "Workspace Lead",
                Description =
                    "Head of the workspace and bears business responsibility for success of the workspace"
            },
            new()
            {
                Id = (int)RoleNames.Admin,
                Name = "Admin",
                Description =
                    "Management authority within the workspace with direct supervision over the cloud resourcing and users"
            },
            new()
            {
                Id = (int)RoleNames.Collaborator,
                Name = "Collaborator",
                Description =
                    "Responsible for contributing to the overall workspace objectives and deliverables"
            },
            new()
            {
                Id = (int)RoleNames.Guest,
                Name = "Guest",
                Description =
                    "Able to view the workspace and its contents but not able to contribute or modify anything"
            }
        };
    }

    public enum RoleNames
    {
        Remove = 1, // default is used in the UI to remove a user from a project
        WorkspaceLead = 2,
        Admin = 3,
        Collaborator = 4,
        Guest = 5
    }
}