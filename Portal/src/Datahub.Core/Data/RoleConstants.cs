using Datahub.Core.Model.Projects;

namespace Datahub.Core.Data;

public static class RoleConstants
{
    public const string ADMIN_SUFFIX = "-admin";
    public const string WORKSPACE_LEAD_SUFFIX = "-workspace-lead";
    public const string COLLABORATOR_SUFFIX = "-collaborator";
    public const string GUEST_SUFFIX = "-guest";
    public const string WEBAPP_SUFFIX = "-wsapp";

    public const string DATAHUB_ADMIN_PROJECT = "DHPGLIST";
    public const string DATAHUB_ROLE_ADMIN = DATAHUB_ADMIN_PROJECT + ADMIN_SUFFIX;
    public const string DATAHUB_ROLE_ADMIN_AS_GUEST = DATAHUB_ADMIN_PROJECT + "-admin-as-guest";

    public static string GetRoleConstants(Project_Role role)
    {
        return role.Id switch
        {
            2 => WORKSPACE_LEAD_SUFFIX,
            3 => ADMIN_SUFFIX,
            4 => COLLABORATOR_SUFFIX,
            5 => GUEST_SUFFIX,
            _ => "role not found"
        };
    }
}