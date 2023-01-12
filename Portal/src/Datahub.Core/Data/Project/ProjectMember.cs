using System;
using System.Collections.Generic;
using Datahub.Core.Model.Datahub;
using Lucene.Net.Analysis.Hunspell;

namespace Datahub.Core.Data.Project;

public class ProjectMember
{
    public string UserId { get; init; }
    public string Email { get; }
    public string Name { get; init; }
    public bool IsAdmin { get; }
    public bool IsDataApprover { get; }
    public DateTime? UserAddedProject { get; }
    
    public ProjectMemberRole Role { get; set; }

    //used for testing
    public ProjectMember()
    {
    }

    public ProjectMember(Datahub_Project_User projectUser)
    {
        Email = projectUser.User_Name;
        IsAdmin = projectUser.IsAdmin;
        IsDataApprover = projectUser.IsDataApprover;
        UserId = projectUser.User_ID;
        UserAddedProject = projectUser.Approved_DT;
        Role = IsDataApprover ? ProjectMemberRole.Publisher : IsAdmin ? ProjectMemberRole.Admin : ProjectMemberRole.Contributor;
    }
    
    
}