using System;
using Microsoft.AspNetCore.Components;

namespace Datahub.Tests.ResourceProvisioner;

public class MockNavigationManager : NavigationManager, IDisposable
{
    public MockNavigationManager()
    {
        //do nothing
    }

    public new virtual void NavigateTo(string uri, bool forceLoad = false)
    {
    }
    protected override void NavigateToCore(string uri, bool forceLoad)
    {
    }

    protected sealed override void EnsureInitialized()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}