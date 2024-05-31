namespace Datahub.SpecflowTests;

public static class Testing
{
    public static readonly Guid CurrentUserGuid = Guid.NewGuid();
    public const string CurrentUserEmail = "test@email.com";
    public static readonly string WorkspaceTenantGuid = Guid.NewGuid().ToString();
    
    public static readonly string SubscriptionName = "Test Subscription 1";
    public static readonly string SubscriptionName2 = "Test Subscription 2";
    
    public static readonly string WorkspaceSubscriptionGuid = Guid.NewGuid().ToString();
    public static readonly string WorkspaceSubscriptionGuid2 = Guid.NewGuid().ToString();
    
    public static readonly string WorkspaceAcronym = Guid.NewGuid().ToString().Replace("-", "")[..5];
    public static readonly string WorkspaceAcronym2 = Guid.NewGuid().ToString().Replace("-", "")[..5];
    
    public const string WorkspaceName = "Test Workspace";
    public const string WorkspaceName2 = "Test Workspace2";
}