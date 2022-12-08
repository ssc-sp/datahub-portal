namespace Datahub.Core.Data
{
    public static class RoleConstants
    {
        public const string ADMIN_SUFFIX = "-admin";
        public const string DATAHUB_ADMIN_PROJECT = "DHPGLIST";
        public const string DATAHUB_ROLE_ADMIN = DATAHUB_ADMIN_PROJECT + ADMIN_SUFFIX;
        public const string DATAHUB_ROLE_ADMIN_AS_GUEST = DATAHUB_ADMIN_PROJECT + "-admin-as-guest";
    }
}
