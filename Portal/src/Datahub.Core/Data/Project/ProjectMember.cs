using System;
using Datahub.Core.Model.Datahub;

namespace Datahub.Core.Data.Project;

public class ProjectMember
{
    public string UserId { get; }
    public string Email { get; }
    public string Name { get; init; }
    public DateTime? UserAddedToProject { get; }
    
    public ProjectMemberRole Role { get; set; }

    //used for testing
    public ProjectMember(string userId, ProjectMemberRole role)
    {
        UserId = userId;
        Role = role;
    }

    public ProjectMember(Datahub_Project_User projectUser)
    {
        Email = projectUser.User_Name;
        UserId = projectUser.User_ID;
        UserAddedToProject = projectUser.Approved_DT;
        Role = projectUser.IsDataApprover ? ProjectMemberRole.Publisher : projectUser.IsAdmin ? 
            ProjectMemberRole.Admin : ProjectMemberRole.Contributor;
    }
}