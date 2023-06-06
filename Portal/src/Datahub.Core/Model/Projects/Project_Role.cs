using System;
using System.Collections;
using System.Collections.Generic;

namespace Datahub.Core.Model.Projects;

public class Project_Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

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
        Remove = 0, // default is used in the UI to remove a user from a project
        WorkspaceLead = 1,
        Admin = 2,
        Collaborator = 3,
        Guest = 4
    }
}