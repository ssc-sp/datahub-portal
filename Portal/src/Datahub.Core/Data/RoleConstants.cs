using Datahub.Core.Model.Projects;

namespace Datahub.Core.Data;

public static class RoleConstants
{
    public const string ADMINSUFFIX = "-admin";
    public const string WORKSPACELEADSUFFIX = "-workspace-lead";
    public const string COLLABORATORSUFFIX = "-collaborator";
    public const string GUESTSUFFIX = "-guest";
    public const string WEBAPPSUFFIX = "-webapp";

    public const string DATAHUBADMINPROJECT = "DHPGLIST";
    public const string DATAHUBROLEADMIN = DATAHUBADMINPROJECT + ADMINSUFFIX;
    public const string DATAHUBROLEADMINASGUEST = DATAHUBADMINPROJECT + "-admin-as-guest";

    public static string GetRoleConstants(ProjectRole role)
    {
        return role.Id switch
        {
            2 => WORKSPACELEADSUFFIX,
            3 => ADMINSUFFIX,
            4 => COLLABORATORSUFFIX,
            5 => GUESTSUFFIX,
            _ => "role not found"
        };
    }
}