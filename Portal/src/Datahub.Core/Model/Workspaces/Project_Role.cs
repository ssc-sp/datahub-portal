namespace Datahub.Core.Model.Projects;

public class Project_Role
{
    /// <summary>
    /// Gets or sets the unique identifier of the project role.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the friendly name of the project role.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets a brief description of the project role's purpose or responsibilities.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this role is applicable for external users.
    /// </summary>
    public bool IsExternalRole { get; set; } = false;

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
                Description = "Revoke the user's access to the workspace",
                IsExternalRole = false
            },
            new()
            {
                Id = (int)RoleNames.WorkspaceLead,
                Name = "Workspace Lead",
                Description =
                    "Head of the workspace and bears business responsibility for success of the workspace",
                IsExternalRole = false
            },
            new()
            {
                Id = (int)RoleNames.Admin,
                Name = "Admin",
                Description =
                    "Management authority within the workspace with direct supervision over the cloud resourcing and users",
                IsExternalRole = false
            },
            new()
            {
                Id = (int)RoleNames.Collaborator,
                Name = "Collaborator",
                Description =
                    "Responsible for contributing to the overall workspace objectives and deliverables",
                IsExternalRole = false
            },
            new()
            {
                Id = (int)RoleNames.Guest,
                Name = "Guest",
                Description =
                    "Able to view the workspace and its contents but not able to contribute or modify anything",
                IsExternalRole = false
            },
            new()
            {
                Id = (int)RoleNames.DisabledUser,
                Name = "Disabled User",
                Description =
                    "A user whose access has been disabled and cannot interact with the workspace",
                IsExternalRole = false
            },
            new()
            {
                Id = (int)RoleNames.WebApp,
                Name = "Web Application Access",
                Description =
                    "Limited access to the web application interface only",
                IsExternalRole = true
            },
            new()
            {
                Id = (int)RoleNames.Storage,
                Name = "Storage",
                Description =
                    "Limited access to storage upload and download",
                IsExternalRole = true
            },
            new()
            {
                Id = (int)RoleNames.WebAppAndStorage,
                Name = "Web Application and Storage",
                Description =
                    "Access to both web application interface and storage resources",
                IsExternalRole = true
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
        DisabledUser = 6,

        /// <summary>
        /// An external user with access limited to the web application interface only.
        /// </summary>
        WebApp = 7,

        /// <summary>
        /// An external user with access limited to storage resources only.
        /// </summary>
        Storage = 8,

        /// <summary>
        /// An external user with access to both web application interface and storage resources.
        /// </summary>
        WebAppAndStorage = 9
    }
}
