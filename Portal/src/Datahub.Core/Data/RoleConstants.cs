using System.Collections.Immutable;
using Datahub.Core.Model.Projects;

namespace Datahub.Core.Data;

public static class RoleConstants
{
    public const string ADMIN_ROLE = "admin";
    public const string ADMIN_SUFFIX = "-" + ADMIN_ROLE;
    public const string WORKSPACE_LEAD_ROLE = "workspace-lead";
    public const string WORKSPACE_LEAD_SUFFIX = "-" + WORKSPACE_LEAD_ROLE;
    public const string COLLABORATOR_ROLE = "collaborator";
    public const string COLLABORATOR_SUFFIX = "-" + COLLABORATOR_ROLE;
    public const string GUEST_ROLE = "guest";
    public const string GUEST_SUFFIX = "-" + GUEST_ROLE;
    public const string WEBAPP_SUFFIX = "-webapp";
    public const string CBR_OWNER_ROLE = "cbr-owner";
    public const string CBR_OWNER_SUFFIX = "-" + CBR_OWNER_ROLE;

    public const string DATAHUB_ADMIN_PROJECT = "DHPGLIST";
    public const string DATAHUB_ROLE_ADMIN = DATAHUB_ADMIN_PROJECT + ADMIN_SUFFIX;
    public const string DATAHUB_ROLE_ADMIN_AS_GUEST = DATAHUB_ADMIN_PROJECT + "-admin-as-guest";

    public const string DATAHUB_APPROVER_PROJECT = "DHAPPRV"; // 7 character max
    public const string DATAHUB_APPROVER_ROLE = DATAHUB_APPROVER_PROJECT + "-approver";

    public const string TRUSTED_ENTRA_LOGIN = "trusted-entra-login";
    public const string EXTERNAL_LOGIN = "external-login";

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

    public static readonly ImmutableHashSet<int> AllowedDataStewardRoleIds = [
        (int)Project_Role.RoleNames.WorkspaceLead,
        (int)Project_Role.RoleNames.Collaborator,
        (int)Project_Role.RoleNames.Admin
    ];
}
