using System;
using System.Collections;
using System.Collections.Generic;

namespace Datahub.Core.Model.Projects;

public class Project_Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    /// <summary>
    /// This should be used to determine if the user is an admin or not.
    /// Note: If you are looking to show or hide UI elements off this, use the "DatahubAuthView" instead.
    /// </summary>
    public bool IsAdmin => Id is (int)RoleNames.Admin or (int)RoleNames.WorkspaceLead;
    
    /// <summary>
    /// This should be used to determine if the user is a collaborator or not.
    /// Note: If you are looking to show or hide UI elements off this, use the "DatahubAuthView" instead.
    /// </summary>
    public bool IsCollaborator => Id is (int)RoleNames.Admin or (int)RoleNames.WorkspaceLead or (int)RoleNames.Collaborator;

    public static IEnumerable<Project_Role> GetAll() => _roles.Value;

    private static Lazy<IEnumerable<Project_Role>> _roles = new(CreateRoles);

    private static IEnumerable<Project_Role> CreateRoles()
    {
        return new List<Project_Role>
        {
            new()
            {
                Id = (int)RoleNames.Remove,
                Name = "Remove User",
                Description = "Revoke the user's access to the project's private resources"
            },
            new()
            {
                Id = (int)RoleNames.WorkspaceLead,
                Name = "Workspace Lead",
                Description =
                    "Head of the business unit and bears business responsibility for successful implementation and availability"
            },
            new()
            {
                Id = (int)RoleNames.Admin,
                Name = "Admin",
                Description =
                    "Management authority within the project with direct supervision over the project resources and deliverables"
            },
            new()
            {
                Id = (int)RoleNames.Collaborator,
                Name = "Collaborator",
                Description =
                    "Responsible for contributing to the overall project objectives and deliverables to ensure success"
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