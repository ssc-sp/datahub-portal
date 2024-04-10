namespace Datahub.SpecflowTests;

public static class Testing
{
    public static readonly Guid CURRENT_USER_GUID = Guid.NewGuid();
    public static readonly string CURRENT_USER_EMAIL = "test@email.com";
    public static readonly string WORKSPACE_SUBSCRIPTION_GUID = Guid.NewGuid().ToString();
    public static readonly string WORKSPACE_ACRONYM = Guid.NewGuid().ToString().Replace("-", "")[..5];
    public static readonly string WORKSPACE_NAME = "Test Workspace";
}