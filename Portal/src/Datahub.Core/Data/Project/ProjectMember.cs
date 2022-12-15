using Datahub.Core.Model.Datahub;

namespace Datahub.Core.Data.Project;

public class ProjectMember
{
    public string UserId { get; }
    public string Email { get; }
    public string Name { get; init; }
    public bool IsAdmin { get; }
    public bool IsDataApprover { get; }

    public ProjectMember(Datahub_Project_User projectUser)
    {
        Email = projectUser.User_Name;
        IsAdmin = projectUser.IsAdmin;
        IsDataApprover = projectUser.IsDataApprover;
        UserId = projectUser.User_ID;
    }
}