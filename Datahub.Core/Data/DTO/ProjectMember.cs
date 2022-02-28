using Datahub.Core.EFCore;

namespace Datahub.Core.Data.DTO;

public class ProjectMember
{
    public string UserId { get; init; }
    public string Email { get; init; }
    public string Name { get; init; }
    public bool IsAdmin { get; init; }
    public bool IsDataApprover { get; init; }

    public ProjectMember(Datahub_Project_User projectUser)
    {
        Email = projectUser.User_Name;
        IsAdmin = projectUser.IsAdmin;
        IsDataApprover = projectUser.IsDataApprover;
        UserId = projectUser.User_ID;
    }
}