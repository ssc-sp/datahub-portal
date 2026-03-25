using System.Collections.Immutable;
using Datahub.Core.Model.Projects;
using static Datahub.Core.Model.Projects.Project_Role;

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
    public const string EXTERNAL_GUEST_ROLE = "extguest";
    public const string EXTERNAL_GUEST_SUFFIX = "-" + GUEST_ROLE;
    public const string WEBAPP_SUFFIX = "-webapp";
    public const string STORAGE_SUFFIX = "-storage";
    public const string CBR_OWNER_ROLE = "cbr-owner";
    public const string CBR_OWNER_SUFFIX = "-" + CBR_OWNER_ROLE;

    public const string DATAHUB_ADMIN_PROJECT = "DHPGLIST";
    public const string DATAHUB_ROLE_ADMIN = DATAHUB_ADMIN_PROJECT + ADMIN_SUFFIX;
    public const string DATAHUB_ROLE_ADMIN_AS_GUEST = DATAHUB_ADMIN_PROJECT + "-admin-as-guest";

    public const string DATAHUB_APPROVER_PROJECT = "DHAPPRV"; // 7 character max
    public const string DATAHUB_APPROVER_ROLE = DATAHUB_APPROVER_PROJECT + "-approver";

    public const string TRUSTED_ENTRA_LOGIN = "trusted-entra-login";
    public const string EXTERNAL_LOGIN = "external-login";

    public static string[] GetRoleSuffixes(Project_Role role)
    {
        return role.Id switch
        {
            (int)RoleNames.WorkspaceLead => [WORKSPACE_LEAD_SUFFIX],
            (int)RoleNames.Admin => [ADMIN_SUFFIX],
            (int)RoleNames.Collaborator => [COLLABORATOR_SUFFIX],
            (int)RoleNames.Guest => [GUEST_SUFFIX],
            (int)RoleNames.WebApp => [WEBAPP_SUFFIX],
            (int)RoleNames.Storage => [STORAGE_SUFFIX],
            (int)RoleNames.WebAppAndStorage => [WEBAPP_SUFFIX, STORAGE_SUFFIX],
            _ => ["role not found"]
        };
    }

    public static readonly ImmutableHashSet<int> AllowedDataStewardRoleIds = [
        (int)Project_Role.RoleNames.WorkspaceLead,
        (int)Project_Role.RoleNames.Collaborator,
        (int)Project_Role.RoleNames.Admin
    ];
}
