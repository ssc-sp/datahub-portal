using System.Reflection;
using Bunit;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Components;

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

    public static readonly string ResourceGroupName1 = $"fsdh_proj_{WorkspaceAcronym}_test_rg";
    public static readonly string ResourceGroupName2 = $"fsdh_proj_{WorkspaceAcronym2}_test_rg";

    public const string StorageAccountId = "storageaccountid";
    public const string BudgetId = "budget-id";

    public const string InvalidWorkspaceAcronym = "invalid-acronym";
    public const string InvalidSubscriptionId = "invalid-subscription-id";

    public const string WorkspaceName = "Test Workspace";
    public const string WorkspaceName2 = "Test Workspace2";

    public static readonly List<string> ServiceNames = new() { "Storage", "Compute", "Network" };

    public static readonly List<DateTime> Dates = new List<DateTime>
        { DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1), DateTime.UtcNow };

    public static void SetPrivateField<T>(IRenderedComponent<T> component, string fieldName, object value)
        where T : IComponent
    {
        var obj = component.Instance;
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    public static void GetPrivateField<T>(IRenderedComponent<T> component, string fieldName, out object? value)
        where T : IComponent
    {
        var obj = component.Instance;
        var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        value = field?.GetValue(obj);
    }
    public static object? InvokePrivateMethod<T>(IRenderedComponent<T> component, string methodName,
        params object[] parameters) where T : IComponent
    {
        var obj = component.Instance;
        var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        return method != null ? method.Invoke(obj, parameters) : null;
    }

    private static async Task<object?> InvokePrivateMethodAsync<T>(IRenderedComponent<T> component,
        string methodName, params object[] parameters) where T : IComponent
    {
        var obj = component.Instance;
        var method = obj.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        return method != null ? await component.InvokeAsync(() => method.Invoke(obj, parameters)) : null;
    }
}