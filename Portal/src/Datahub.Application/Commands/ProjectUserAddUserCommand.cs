using Datahub.Core.Model.Achievements;

namespace Datahub.Application.Commands;

public class ProjectUserAddUserCommand
{
    // guid with all 0's
    public static string NEW_USER_GUID = default(Guid).ToString();
    public string ProjectAcronym { get; set; }
    public string GraphGuid { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public int RoleId { get; set; }
    
    public PortalUser? PortalUser { get; set; }
}