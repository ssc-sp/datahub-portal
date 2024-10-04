using Datahub.Core.Data;

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
    public const string ExistingWorkspaceAcronym = "test";
    public const string InvalidWorkspaceAcronym = "invalid-acronym";
    public const string InvalidSubscriptionId = "invalid-subscription-id";
    
    public const string WorkspaceName = "Test Workspace";
    public const string WorkspaceName2 = "Test Workspace2";

    public static readonly List<string> ServiceNames = new() { "Storage", "Compute", "Network" };
    public const string ExistingTestRg = "fsdh-static-test-rg";
    public const string ExistingTestRg2 = "fsdh_proj_test_test_rg";
    public const string ExistingTestBudget = $"/subscriptions/{{SUBSCRIPTION}}/resourceGroups/{ExistingTestRg}/providers/Microsoft.Consumption/budgets/{ExistingTestRg}-budget";
    public const decimal ExistingTestRgTotal = (decimal)0.03; // 3 cents as of August 27 2024
    public static readonly List<DateTime> Dates = new List<DateTime> { DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), DateTime.UtcNow };
    
    public const string MockCosts = "costs-mock.json";
    public const string MockTotals = "totals-mock.json";
}