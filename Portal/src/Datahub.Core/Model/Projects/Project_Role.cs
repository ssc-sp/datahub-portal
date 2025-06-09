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
                Id = (int)RoleNames.Removed,
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
            },
            new()
            {
                Id = (int)RoleNames.DisabledUser,
                Name = "Disabled User",
                Description =
                    "A user whose access has been disabled and cannot interact with the workspace"
            }
        };
    }

    /// <summary>
    /// Defines a set of role names representing different permission levels within a workspace.
    /// </summary>
    public enum RoleNames
    {
        /// <summary>
        /// Represents a soft deletion marker to remove the user from the workspace.
        /// </summary>
        Removed = 1,

        /// <summary>
        /// The lead for the workspace, bearing overall business responsibility.
        /// </summary>
        WorkspaceLead = 2,

        /// <summary>
        /// A workspace administrator who manages resources and users.
        /// </summary>
        Admin = 3,

        /// <summary>
        /// A collaborator who contributes to the workspace's objectives.
        /// </summary>
        Collaborator = 4,

        /// <summary>
        /// A viewer with read-only access to workspace contents.
        /// </summary>
        Guest = 5,

        /// <summary>
        /// A disabled user with no privileges within the workspace.
        /// </summary>
        DisabledUser = 6
    }
}