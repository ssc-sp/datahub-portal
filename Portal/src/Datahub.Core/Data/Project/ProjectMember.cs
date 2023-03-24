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
        Role = projectUser.IsDataApprover ? ProjectMemberRole.WorkspaceLead : projectUser.IsAdmin ? 
            ProjectMemberRole.Admin : ProjectMemberRole.Collaborator;
    }

    public ProjectMember(GraphUser graphUser, ProjectMemberRole role = ProjectMemberRole.Collaborator)
    {
        Email = graphUser.Mail;
        UserId = graphUser.Id;
        Role = role;
        UserAddedToProject = DateTime.Now;
    }
}